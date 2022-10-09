using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Inven")]
public class PlayerInvenSO : ScriptableObject
{
    #region PrivateVariables
    private Dictionary<MineralType, int> m_playerMineralInven = new Dictionary<MineralType, int>();
    private Dictionary<MineralType, int> m_mineralMaxCount = new Dictionary<MineralType, int>();

    [SerializeField] private PlayerMineralUIChannelSO m_playerCazelinUIChannelSO;
    [SerializeField] private PlayerMineralUIChannelSO m_playerStarLightUIChannelSO;

    private const string m_upgradeCazelinID = ConstStringStorage.UPGRADE_ID_INVEN_CAZELIN_CAPACITY;
    private const string m_upgradeStarlightID = ConstStringStorage.UPGRADE_ID_INVEN_STARLIGHT_CAPACITY;
    private int m_cazelinInvenLevel = 1;
    private int m_starlightInvenLevel = 1;

    public delegate void MineralChangedHandler(MineralType mineralType, int amount, int maxAmount);
    public event MineralChangedHandler OnChangedMineral;

    #endregion

    private void OnEnable()
    {
        m_cazelinInvenLevel = 1;
        m_starlightInvenLevel = 1;

    }

    #region PublicVariables

    #endregion
    public void Initialize()
    {
        m_playerMineralInven.Clear();
        m_mineralMaxCount.Clear();

        m_playerMineralInven.Add(MineralType.Cazelin, 0);
        m_playerMineralInven.Add(MineralType.Starlight, 0);

        m_mineralMaxCount.Add(MineralType.Cazelin, 300);
        m_mineralMaxCount.Add(MineralType.Starlight, 300);

        m_cazelinInvenLevel = 1;
        m_starlightInvenLevel = 1;

        UpdateMineralText(MineralType.Cazelin);
        UpdateMineralText(MineralType.Starlight);
    }

    private void UpdateMineralText(MineralType _mineralType)
    {
        OnChangedMineral.Invoke(_mineralType, GetCountOfMineral(_mineralType), m_mineralMaxCount[_mineralType]);
        switch (_mineralType)
        {
            case MineralType.Cazelin:
                m_playerCazelinUIChannelSO.RaiseEvent(GetCountOfMineral(_mineralType), m_mineralMaxCount[_mineralType]);
                break;
            case MineralType.Starlight:
                m_playerStarLightUIChannelSO.RaiseEvent(GetCountOfMineral(_mineralType), m_mineralMaxCount[_mineralType]);
                break;
            default:
                break;
        }
    }
    #region PublicMethod

    public int GetCountOfMineral(MineralType _mineralType)
    {
        if (m_playerMineralInven.ContainsKey(_mineralType))
            return m_playerMineralInven[_mineralType];

        return -1;
    }

    public float AddMineral(MineralType _mineralType, int _count)
    {
        float gain = _count;
        if (m_playerMineralInven[_mineralType] == m_mineralMaxCount[_mineralType])
            return 0;

        if (!m_playerMineralInven.ContainsKey(_mineralType))
            m_playerMineralInven.Add(_mineralType, _count);
        else
            m_playerMineralInven[_mineralType] += _count;

        if (m_playerMineralInven[_mineralType] > m_mineralMaxCount[_mineralType])
        {
           int over = m_playerMineralInven[_mineralType] - m_mineralMaxCount[_mineralType];
            gain -= over;
            m_playerMineralInven[_mineralType] = m_mineralMaxCount[_mineralType];
        }
            
        UpdateMineralText(_mineralType);
        return gain;
    }
      
    public void RemoveMineral(MineralType _mineralType, int _count)
    {
        if (m_playerMineralInven.ContainsKey(_mineralType))
            m_playerMineralInven[_mineralType] -= _count;

        if (m_playerMineralInven[_mineralType] < 0)
            m_playerMineralInven[_mineralType] = 0;

        UpdateMineralText(_mineralType);
    }

    /// <summary>
    /// _count 만큼 최대량 업그레이드
    /// </summary>
    /// <param name="_mineralType"></param>
    /// <param name="_count"></param>
    public void UpgradeMaxMineral(MineralType _mineralType, int _count)
    {
        if (_count < 0)
            _count = int.MaxValue;

        if (!m_mineralMaxCount.ContainsKey(_mineralType))
            return;

        m_mineralMaxCount[_mineralType] = _count;
        UpdateMineralText(_mineralType);
    }


    public int GetLevel(string _id)
    {
        if (_id.Equals(m_upgradeCazelinID))
            return m_cazelinInvenLevel;

        return m_starlightInvenLevel;
    }

    public void SetUpgradedInfo(float _value, string _id)
    {
        if (_id.Equals(m_upgradeCazelinID))
        {
            UpgradeMaxMineral(MineralType.Cazelin, (int)_value);
            m_cazelinInvenLevel++;
        }
        else
        {
            UpgradeMaxMineral(MineralType.Starlight, (int)_value);
            m_starlightInvenLevel++;
        }
    }

    public void SetInitialCapacity(float _value, string _id)
    {
        if (_id.Equals(m_upgradeCazelinID))
            UpgradeMaxMineral(MineralType.Cazelin, (int)_value);
        else
            UpgradeMaxMineral(MineralType.Starlight, (int)_value);

    }

    #endregion
}
