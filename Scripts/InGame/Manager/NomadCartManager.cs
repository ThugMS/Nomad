using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public interface IAllCartUsable
{
    public void PlayCazelinParticle();
    public void TransferOwnerShip(int _actorID = 0);
}
public interface IRPCCartAddable
{
    public void CallRPCAddCart(GameObject _obj);
}
public class NomadCartManager : MonoBehaviourPun, IPunObservable, IAllCartUsable, IRPCCartAddable
{

    #region PrivateVariables

    private List<NomadCartBase> m_carts;
    private List<PhotonView> m_photonViews = new List<PhotonView>();


    private float m_baseSpeed = NomadConstants.SPEED;
    private float m_speedWeight = NomadConstants.SPEEDWEIGHT;
    private float m_engineCartTurnSpeed = NomadConstants.ENGINE_TURN_SPEED;

    [SerializeField] private IntEventChannelSO m_gameOverEvent;
    [SerializeField] private IntEventChannelSO m_dangerousHpChannelSO;
    [SerializeField] private IntEventChannelSO m_getCartChannelSO;

    private NomadEngineCart m_engineCart;
    private Transform m_nomadTransform;

    private float m_MaxHp;
    #endregion

    #region Protected Variables
    #endregion

    #region PublicVariables

    #endregion

    void Awake()
    {
        m_carts = new List<NomadCartBase>();
    }

    private void Update()
    {
        
    }

    #region PrivateMethod
    private void CreateCart(NomadCartBase.Type _type)
    {
        //Cart 초기 생성
        GameObject obj = PhotonNetwork.InstantiateRoomObject(ConstStringStorage.NOMAD_FOLDER_PATH + _type.ToString(), Vector3.zero, Quaternion.identity) as GameObject;

        CallRPCAddCart(obj);
    }

    private void RaiseDangerHpEvent(int _percent)
    {
        m_dangerousHpChannelSO.RaiseEvent(_percent);
    }
    private void InitKindOfCartConfig(NomadCartBase _cart)
    {
        AddCartEvent(_cart);
        if (_cart is NomadEngineCart engineCart)
        {
            m_engineCart = engineCart;
            m_engineCart.SetEngineCartConfig(this, m_engineCartTurnSpeed);
        }
        else if (_cart is NomadMineralCart mineralCart)
        {

        }
        else if(_cart is NomadWeaponCart weaponCart)
        {

        }
    }
    private void AddCartEvent(NomadCartBase _cart)
    {
        if (_cart is NomadEngineCart engineCart)
        {
            engineCart.OnDangerHp += RaiseDangerHpEvent;
            engineCart.OnDie += CallGameOverEvent;

        }
        else if (_cart is NomadMineralCart mineralCart)
            _cart.OnChangedMaxHp += SetEngineCartMaxHp;
        
        else if (_cart is NomadWeaponCart weaponCart)
            _cart.OnChangedMaxHp += SetEngineCartMaxHp;
        
    }
    private void RemoveCartEvent(NomadCartBase _cart)
    {
        if (_cart is NomadEngineCart engineCart)
        {
            engineCart.OnDangerHp -= RaiseDangerHpEvent;
            engineCart.OnDie -= CallGameOverEvent;

        }
        else if (_cart is NomadMineralCart mineralCart)
            _cart.OnChangedMaxHp -= SetEngineCartMaxHp;

        else if (_cart is NomadWeaponCart weaponCart)
            _cart.OnChangedMaxHp -= SetEngineCartMaxHp;

    }
    private void CallGameOverEvent(int _type)
    {
        m_gameOverEvent.RaiseEvent(_type);
    }
    [PunRPC]
    private void RPC_SetNomadTransform(int _viewID)
    {
        m_nomadTransform = PhotonView.Find(_viewID).transform;
    }
    [PunRPC]
    private void UseAllCartFuelParticle()
    {
        foreach (NomadCartBase cart in m_carts)
            cart.UseFuelParticle();
    }
    private void SetEngineCartMaxHp()
    {
        float allHp = 0;
        for (int i = 1; i < m_carts.Count; i++)
            allHp += m_carts[i].GetMaxHp();

        m_engineCart.CallRPCSetMaxHp(allHp * NomadConstants.ENGINE_MAXHP_PLUSVALUE);
    }
    private void OnDestroy()
    {
        foreach (NomadCartBase cart in m_carts)
            RemoveCartEvent(cart);
    }

    private void OnPostAddCart()
    {
        //엔진칸 길이에 따른 가중치 증가
        m_engineCart.SetCazelinUseWeight(m_carts.Count);
        SetSpeedCarts(m_baseSpeed + m_carts.Count * m_speedWeight);
    }
    private void SetSpeedCarts(float _speed)
    {
        foreach (NomadCartBase cart in m_carts)
            cart.SetSpeed(_speed);
    }
    #endregion

    #region ProtectedMethod
    #endregion

    #region PublicMethod
    public void CallRPCAddCart(GameObject _obj)
    {
        photonView.RPC(nameof(RPC_AddCart), RpcTarget.AllBuffered, _obj.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    public void RPC_AddCart(int _viewID)
    {
        
        //각 클라이언트에 Car 추가
        //_viewID에 해당하는 오브젝트를 찾아서 추가함.
        NomadCartBase cart = PhotonView.Find(_viewID).gameObject.GetComponent<NomadCartBase>();

        //이미 연결된 카트라면 return;
        if (m_carts.Contains(cart))
            return;

        if (m_carts.Count >= NomadConstants.CART_BASE_COUNT)
            m_getCartChannelSO.RaiseEvent((int)cart.GetCartType());

        m_photonViews.Add(cart.GetPhotonView());
        m_carts.Add(cart);

        //cart별 세팅
        InitKindOfCartConfig(cart);

        cart.InitCarConfig(m_nomadTransform, m_carts.Count,
            m_carts.Count > 1 ? m_carts[m_carts.Count - 2] : null);

        OnPostAddCart();
    }
    public void PlayCazelinParticle() 
    { 
        photonView.RPC(nameof(UseAllCartFuelParticle), RpcTarget.AllBuffered);
    }
    public int GetNomadSize()
    {
        return m_carts.Count;
    }
    public NomadCartBase GetCart(int _index)
    {
        if (m_carts.Count <= _index)
            return null;

        return m_carts[_index];
    }
    public void SetActiveAllHpBar(bool on)
    {
        foreach (NomadCartBase car in m_carts)
            car.SetActiveHpBar(on);
    }
    public void TransferOwnerShip(int _actorID = 0)
    {
        foreach (PhotonView pv in m_photonViews)
        {
            pv.TransferOwnership(_actorID);
        }
    }
    public void CreateShip()
    {
        GameObject obj = PhotonNetwork.InstantiateRoomObject(ConstStringStorage.NOMAD_PATH, Vector3.zero, Quaternion.identity) as GameObject;

        //각 클라이언트에 노마드 transform설정
        photonView.RPC(nameof(RPC_SetNomadTransform), RpcTarget.AllBuffered, obj.GetPhotonView().ViewID);

        //초기 cart설정
        CreateCart(NomadCartBase.Type.EngineCart);
        CreateCart(NomadCartBase.Type.StarlightCart);
        CreateCart(NomadCartBase.Type.CazelinCart);
    }
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

}
