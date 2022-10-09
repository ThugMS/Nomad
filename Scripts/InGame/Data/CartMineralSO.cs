using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

interface IMineralManagable
{
    public bool IsFull(MineralType _mineralType);
    public int PutMineral(int _senderViewId, MineralType _mineralType, int _amount);
    public bool CanSpendMineral(MineralType _mineralType, int _cost);
    public void SpendMineral(MineralType _mineralType, int _cost);
}
public class MineralInfo
{
    public MineralType mineralType;
    public int amount = 0;

    public MineralInfo(MineralType mineralType, int amount)
    {
        this.mineralType = mineralType;
        this.amount = amount;
    }
}
[CreateAssetMenu(menuName = "Datas/CartMineral")]
public class CartMineralSO : ScriptableObject
{
    private Dictionary<MineralType, int> m_mineralAmount = new Dictionary<MineralType, int>();
    private Dictionary<MineralType, int> m_mineralMaxAmount = new Dictionary<MineralType, int>();

    [SerializeField] private TotalMineraUIChannelSO m_totalMineralUIChannelSO;
    [SerializeField] private VoidEventChannelSO m_onChangedAmountChannelSO;
    [SerializeField] private MineralRequestResultEventChannelSO m_onReicevedSpendMineralResultChannelSO;


    private void OnDisable()
    {
    }

    private void SetMineral(MineralType _mineralType, int _amount)
    {
        if (!m_mineralAmount.ContainsKey(_mineralType))
            return;

        m_mineralAmount[_mineralType] = _amount;
        m_totalMineralUIChannelSO.RaiseEvent(_mineralType, GetCountOfMineral(_mineralType), GetCountOfMaxMineral(_mineralType));
        m_onChangedAmountChannelSO.RaiseEvent();
    }
    private void SetMaxMineral(MineralType _mineralType, int _maxAmount)
    {
        if (!m_mineralMaxAmount.ContainsKey(_mineralType))
            return;
        m_mineralMaxAmount[_mineralType] = _maxAmount;
        
        if (GetCountOfMaxMineral(_mineralType) < GetCountOfMineral(_mineralType))
            SetMineral(_mineralType, GetCountOfMaxMineral(_mineralType));

        m_totalMineralUIChannelSO.RaiseEvent(_mineralType, GetCountOfMineral(_mineralType), GetCountOfMaxMineral(_mineralType));
    }
    public void SendRaiseEventChangeMineral(int _actorNum, int _ObjectId, MineralType _mineralType, int _amount)
    {
        int eventcode = 1;
        object[] data = new object[] { _actorNum, _ObjectId, _mineralType, _amount };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

        PhotonNetwork.RaiseEvent((byte)eventcode, data, raiseEventOptions, SendOptions.SendReliable);
    }
    public void SendRaiseEventSpendMineralByButton(int _actorNum,int _objectId, List<MineralType> _mineralTypes, List<int> _amounts)
    {
        int eventcode = 2;
        object[] data = new object[] { _actorNum, _objectId, _amounts.ToArray()};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

        PhotonNetwork.RaiseEvent((byte)eventcode, data, raiseEventOptions, SendOptions.SendReliable);
    }
    public void OnChangeMineralEvent(EventData photonEvent)
    {
        int code = photonEvent.Code;

        if (code != 1 && code != 2)
            return;
        object[] data = (object[])photonEvent.CustomData;

        if(code == 1)
        {
            int amount = (int)data[3];
            if (amount >= 0)
                SendRaisePutMineralEvent((int)data[0], (int)data[1], AddMineral((MineralType)data[2], amount));
            else
                m_onReicevedSpendMineralResultChannelSO.RaiseEvent((int)data[0], (int)data[1], RemoveMineral((MineralType)data[2], -amount));
        }
        else if(code == 2)
        {
            m_onReicevedSpendMineralResultChannelSO.RaiseEvent((int)data[0], (int)data[1], RemoveMineral((int[])data[2]));
        }
    }
 
    public void SendRaisePutMineralEvent(int _actorId, int _senderViewId, bool _isSuccess)
    {
        if (_actorId != PhotonNetwork.LocalPlayer.ActorNumber)
            return;
        int eventcode = 20;
        object[] data = new object[] { _actorId, _senderViewId, _isSuccess };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)eventcode, data, raiseEventOptions, SendOptions.SendReliable);
    }

    public int GetCountOfMaxMineral(MineralType _mineralType)
    {
        if (!m_mineralMaxAmount.ContainsKey(_mineralType))
            return -1;

        return m_mineralMaxAmount[_mineralType];
    }

    public void ChangeMaxMineral(MineralType _mineralType, int _changedAmount)
    {
        SetMaxMineral(_mineralType, GetCountOfMaxMineral(_mineralType) + _changedAmount);
    }

    public int GetCountOfMineral(MineralType _mineralType)
    {
        if (!m_mineralAmount.ContainsKey(_mineralType))
            return -1;

        return m_mineralAmount[_mineralType];

    }
    public bool AddMineral(MineralType _mineralType, int _changedAmount)
    {
        if (GetCountOfMineral(_mineralType) + _changedAmount > GetCountOfMaxMineral(_mineralType))
            return false;

        SetMineral(_mineralType, GetCountOfMineral(_mineralType) + _changedAmount);
        return true;

    }
    public bool RemoveMineral(MineralType _mineralType, int _changedAmount)
    {
        if (GetCountOfMineral(_mineralType) - _changedAmount < 0)
            return false;

        SetMineral(_mineralType, GetCountOfMineral(_mineralType) - _changedAmount);
        return true;
    }
    public bool RemoveMineral(int[] _changedAmounts)
    {
        for (int i=0; i< _changedAmounts.Length; i++)
        {
            if (GetCountOfMineral((MineralType)i) - _changedAmounts[i] < 0)
                return false;
        }

        for (int i = 0; i < _changedAmounts.Length; i++)
            SetMineral((MineralType)i, GetCountOfMineral((MineralType)i) - _changedAmounts[i]);
        return true;
    }
    public bool CanSpendMineral(List<MineralType> _mineralTypes, List<int> _costs)
    {
        for (int i = 0; i < _mineralTypes.Count; i++)
        {
            if (!CanSpendMineral(_mineralTypes[i], _costs[i]))
                return false;
        }

        return true;
    }
    public bool CanSpendMineral(MineralType _mineralType, int _cost)
    {
        if (!m_mineralAmount.ContainsKey(_mineralType))
            return false;
        return m_mineralAmount[_mineralType] >= _cost;
    }
    public void Initialize()
    {
        m_onReicevedSpendMineralResultChannelSO = Resources.Load<MineralRequestResultEventChannelSO>(ConstStringStorage.SPENDMINERAL_PATH);
        PhotonNetwork.NetworkingClient.EventReceived += OnChangeMineralEvent;
        if (m_mineralAmount.Count == 0)
        {
            m_mineralAmount.Add(MineralType.Cazelin, 0);
            m_mineralAmount.Add(MineralType.Starlight, 0);
        }
        else
        {
            m_mineralAmount[MineralType.Cazelin] = 0;
            m_mineralAmount[MineralType.Starlight] = 0;
        }

        if (m_mineralMaxAmount.Count == 0)
        {
            m_mineralMaxAmount.Add(MineralType.Cazelin, 0);
            m_mineralMaxAmount.Add(MineralType.Starlight, 0);
        }
        else
        {
            m_mineralMaxAmount[MineralType.Cazelin] = 0;
            m_mineralMaxAmount[MineralType.Starlight] = 0;
        }

    }
    public void Reset()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnChangeMineralEvent;
    }
}