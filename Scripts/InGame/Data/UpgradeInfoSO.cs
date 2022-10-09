using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
public interface IUpgradeable
{ 
    public int GetLevel(string _id);
    public void SetUpgradedInfo(string _id);
}

[CreateAssetMenu(menuName = "Datas/UpgradeInfo")]
public class UpgradeInfoSO : ScriptableObject, IOnEventCallback
{
    #region PrivateVariable
    [SerializeField] private TextAsset m_upgradeTextAsset;

    private Dictionary<string, UpgradeInfoByLevels> m_commonUpgradeInfos = new Dictionary<string, UpgradeInfoByLevels>();
    private Dictionary<string, UpgradeInfoByLevels> m_individualUpgradeInfos = new Dictionary<string, UpgradeInfoByLevels>();
    private Dictionary<string, UpgradeInfoByLevels> m_totalUpgradeInfos = new Dictionary<string, UpgradeInfoByLevels>();

    private UpgradeInfo[] m_upgradeInfos;
    #endregion

    private void Awake()
    {
        LoadData();
    }

    private void OnEnable()
    {
        LoadData();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    #region PrivateMethod

    private void AddUpgradeInfo(Dictionary<string, UpgradeInfoByLevels> _dic, string _id, UpgradeInfo _upgradeInfo)
    {
        _dic.Add(_id, new UpgradeInfoByLevels());
        _dic[_id].AddUpgradeInfo(_upgradeInfo);
    }
    #endregion

    #region PublicMethod
    public void LoadData()
    {
        m_upgradeInfos = JsonParser.ParseToTArrayContainsItem<UpgradeInfo>(m_upgradeTextAsset.text);

        m_totalUpgradeInfos.Clear();
        m_individualUpgradeInfos.Clear();
        m_commonUpgradeInfos.Clear();

        for (int i = 0; i < m_upgradeInfos.Length; i++)
        {
            UpgradeInfo info = m_upgradeInfos[i];
            if (info.GetID().Contains("common"))
                AddUpgradeInfo(m_commonUpgradeInfos, info.GetID(), info);
            else
                AddUpgradeInfo(m_individualUpgradeInfos, info.GetID(), info);

            AddUpgradeInfo(m_totalUpgradeInfos, info.GetID(), info);
        }
    }
    public CostInfo GetCostInfo(string _upgradeId, int _currentLevel)
    {
        if (!m_totalUpgradeInfos.ContainsKey(_upgradeId))
        { 
            Debug.LogError($"{_upgradeId}에 해당하는 업그레이드 정보가 없습니다");
            return new CostInfo();
        }

        UpgradeInfoByLevels levelInfo = m_totalUpgradeInfos[_upgradeId];
        UpgradeInfo info = levelInfo.GetCurrentLevelUpgradeInfo(_currentLevel);

        if (info == null)
            return new CostInfo();

        return new CostInfo(info.GetUsingMineral(), info.GetMineralCost());
    }
    public string GetUpgradeName(string _upgradeId)
    {
        if (!m_totalUpgradeInfos.ContainsKey(_upgradeId))
        {
            Debug.LogError($"{_upgradeId}에 해당하는 업그레이드 정보가 없습니다");
            return "";
        }

        UpgradeInfoByLevels levelInfo = m_totalUpgradeInfos[_upgradeId];
        UpgradeInfo info = levelInfo.GetCurrentLevelUpgradeInfo(1);

        if (info == null)
            return "";

        return info.GetName();
    }
    public float GetUpgradeValue(string _upgradeId, int _targetLevel)
    {
        if (!m_totalUpgradeInfos.ContainsKey(_upgradeId))
        {
            Debug.LogError($"{_upgradeId}에 해당하는 업그레이드 정보가 없습니다");
            return -1;
        }

        UpgradeInfoByLevels levelInfo = m_totalUpgradeInfos[_upgradeId];
        return levelInfo.GetTargetLevelUpgradeValue(_targetLevel);
    }
    public void SendRaiseEvent()
    {
        byte eventcode = 0;
        object[] data = m_upgradeInfos;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(eventcode, data, raiseEventOptions, SendOptions.SendReliable);
    }
    public void OnEvent(EventData photonEvent)
    {
        byte code = photonEvent.Code;

        if (code != 0)
            return;

        m_upgradeInfos = (UpgradeInfo[])photonEvent.CustomData;
    }
    #endregion
}
public class UpgradeInfoByLevels
{
    private List<UpgradeInfo> m_upgradeInfos = new List<UpgradeInfo>();

    public void AddUpgradeInfo(UpgradeInfo _upgradeInfo)
    {
        m_upgradeInfos.Add(_upgradeInfo);
    }

    public UpgradeInfo GetCurrentLevelUpgradeInfo(int _currentLevel)
    {
        if(_currentLevel < 1)
        {
            Debug.LogError($"{_currentLevel}이 1보다 작습니다");
            return null;
        }

        if(_currentLevel > m_upgradeInfos.Count)
        {
            Debug.LogError($"{m_upgradeInfos[0].GetID()}이 현재 가능한 최대 레벨보다 높습니다");
            Debug.LogError($"{_currentLevel}이 현재 가능한 최대 레벨보다 높습니다");
            return null;
        }

        int index = _currentLevel - 1;

        return m_upgradeInfos[index];
    }
    public float GetTargetLevelUpgradeValue(int _targetLevel)
    {
        if (_targetLevel < 1)
        {
            Debug.LogError($"{_targetLevel}이 1보다 작습니다");
            return -1;
        }

        if (_targetLevel > m_upgradeInfos.Count + 1)
        {
            Debug.LogError($"{_targetLevel}로 레벨업 할 수 없습니다");
            return -1;
        }

        if(_targetLevel == m_upgradeInfos.Count + 1)
        {
            int lastIdx = m_upgradeInfos.Count - 1;
            UpgradeInfo newInfo = m_upgradeInfos[lastIdx].CreateNextUpgradeInfo();
            m_upgradeInfos.Add(newInfo);
            return newInfo.GetCurrentValue();
        }

        int index = _targetLevel - 1;
        return m_upgradeInfos[index].GetCurrentValue();
    }
}

[System.Serializable]
public class UpgradeInfo
{
    [SerializeField] private string m_id = "";
    [SerializeField] private string m_name = "";
    [SerializeField] private float m_upgradeCostMultiplier = 1.5f;
    [SerializeField] private float m_upgradeValueMultiplier = 1.5f;
    [SerializeField] private float m_currentValue = 1;
    [SerializeField] private List<MineralType> m_usingMineral = new List<MineralType> { MineralType.Cazelin, MineralType.Starlight };
    [SerializeField] private List<int> m_costMineralCost = new List<int>();

    private UpgradeInfo(string _id, string _name, float _costMultiplier, float _valueMultiplier, float _value, List<MineralType> _usingMineral, List<int> _costMineralCost)
    {
        m_id = _id;
        m_name = _name;
        m_upgradeCostMultiplier = _costMultiplier;
        m_upgradeValueMultiplier = _valueMultiplier;
        m_currentValue = _value;
        m_usingMineral = _usingMineral;
        m_costMineralCost = _costMineralCost;
    }
    
    #region Get
    public float GetCurrentValue()
    {
        return m_currentValue;
    }
    public string GetName()
    {
        return m_name;
    }
    public string GetID() => m_id;
    public List<MineralType> GetUsingMineral() => m_usingMineral;
    public List<int> GetMineralCost() => m_costMineralCost;
    #endregion
    public UpgradeInfo CreateNextUpgradeInfo()
    {
        List<int> nextCosts = new List<int>();

        //업그레이드 비용 증가
        for (int i = 0; i < m_costMineralCost.Count; i++)
            nextCosts.Add((int)(m_costMineralCost[i] * m_upgradeCostMultiplier));

        UpgradeInfo upgradeInfo = new UpgradeInfo(
            m_id,
            m_name,
            m_upgradeCostMultiplier,
            m_upgradeValueMultiplier,
            m_currentValue * m_upgradeValueMultiplier,
            m_usingMineral,
            nextCosts
            );
        return upgradeInfo;
    }
}

public struct CostInfo
{
    public List<MineralType> mineralTypes;
    public List<int> costs;

    public CostInfo(List<MineralType> _types, List<int> _costs)
    {
        this.mineralTypes = _types;
        this.costs = _costs;
    }
}
