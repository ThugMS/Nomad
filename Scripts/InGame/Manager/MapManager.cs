using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;


public class MapManager : MonoBehaviourPunCallbacks, IPunObservable, IManageable, IManageMineralArray
{
    #region PrivateVariables
    private int m_mapSize;
    private int m_mapArrayScale;
    private int m_mapCreateTrigger;
    private int[] m_curPlayerArrayX = {500,500,500,500};
    private int[] m_curPlayerArrayY = {500,500,500,500};
    private bool m_mapChunkChange = false;
    private bool m_isAI = false;
    
    private MapInitialCreate m_getMapInitialCreate;
    private Queue<int> m_randomSeedQueue = new Queue<int>();
    protected PhotonView m_photonView;
    private MineralPosition m_mapVariables = new MineralPosition();
    #endregion

    #region ProtectedVariable
    protected GameObject[,] m_mapChunkArray = new GameObject[1000, 1000];
    protected List<GameObject> m_MapResourceList = new List<GameObject>();
    protected int m_mapResourceCnt = 0;
    #endregion

    protected virtual void Awake()
    {
        m_mapSize = m_mapVariables.GetMapArrayPos();
        m_mapArrayScale = m_mapVariables.GetMapArrayScale();
        m_getMapInitialCreate = FindObjectOfType<MapInitialCreate>();
        m_photonView = GetComponent<PhotonView>();

        m_mapCreateTrigger = m_mapSize / 2 - MapConstants.CREATE_TRIGGER_SIZE;
        m_getMapInitialCreate.AllocateInterface(this);
    }

    #region PrivateMethod
    private void CheckXLineMap(float _playerPosX, float _centerArrayMapX, int _mapPosX, int _mapPosY)
    {
        _mapPosX = _mapPosX + (int)Mathf.Sign(_playerPosX - _centerArrayMapX);

        if (this.m_getMapInitialCreate.GetIsCreate(_mapPosX, _mapPosY) == 0)
            CreateMap(_mapPosX, _mapPosY);
    }

    private void CheckYLineMap(float _playerPosY, float _centerArrayMapY, int _mapPosX, int _mapPosY)
    {
        _mapPosY = _mapPosY + (int)Mathf.Sign(_playerPosY - _centerArrayMapY);

        if (this.m_getMapInitialCreate.GetIsCreate(_mapPosX, _mapPosY) == 0)
            CreateMap(_mapPosX, _mapPosY);
    }

    private void CheckXYLineMap(float _playerPosX, float _centerArrayMapX, float _playerPosY, float _centerArrayMapY, int _mapPosX, int _mapPosY)
    {
        _mapPosX = _mapPosX + (int)Mathf.Sign(_playerPosX - _centerArrayMapX);
        _mapPosY = _mapPosY + (int)Mathf.Sign(_playerPosY - _centerArrayMapY);

        if (this.m_getMapInitialCreate.GetIsCreate(_mapPosX, _mapPosY) == 0)
            CreateMap(_mapPosX, _mapPosY);
    }

    protected void CreateMap(int _mapPosX, int _mapPosY)
    {
        
        this.m_getMapInitialCreate.SetIsCreate(_mapPosX, _mapPosY, 1);
        m_photonView.RPC(nameof(RPC_isCreateUpdate), RpcTarget.AllBuffered, (int)_mapPosX, (int)_mapPosY);
        int randomNumber = (int)Random.Range(0, 100000f);

        m_photonView.RPC(nameof(RPC_CreateMap), RpcTarget.AllBuffered, randomNumber, _mapPosX - m_mapArrayScale / 2, _mapPosY - m_mapArrayScale / 2);
    }

    private int GetMapPosition(float _playerPos)
    {
        int mapPos =  (int)((_playerPos + m_mapSize / 2) / m_mapSize) + m_mapArrayScale / 2;

        if (_playerPos < -(m_mapSize / 2 - 1)) mapPos--;

        return mapPos;
    }

    private void SetActiveGameOverMap(int _cameraPosX, int _cameraPosY, int enginePosX, int enginePosY)
    {
        if (_cameraPosX == enginePosX && _cameraPosY == enginePosY)
            return;

        Debug.Log(_cameraPosX + " " + _cameraPosY);

        for (int i = _cameraPosY - 1; i <= _cameraPosY + 1; i++) 
        {
            for (int j = _cameraPosX - 1; j <= _cameraPosX + 1; j++) 
            {
                if (m_mapChunkArray[i, j] == null)
                    continue;

                m_mapChunkArray[i, j].SetActive(true);
            }
        }

        if(Mathf.Abs(_cameraPosX -enginePosX) > Mathf.Abs(_cameraPosY - enginePosY))
            _cameraPosX = _cameraPosX + (int)Mathf.Sign(enginePosX - _cameraPosX);

        else
            _cameraPosY = _cameraPosY + (int)Mathf.Sign(enginePosY - _cameraPosY);

        SetActiveGameOverMap(_cameraPosX, _cameraPosY, enginePosX, enginePosY);
    }

    [PunRPC]
    private void RPC_CreateMap(int _randomNumber, int _posX, int _posY)
    {
        MapInitialCreate makeMap = GetComponent<MapInitialCreate>();

        Random.InitState(_randomNumber);
        makeMap.SetMapResourceCnt(m_mapResourceCnt);
        makeMap.NoiseCreateMap(_posX, _posY, _randomNumber);
        m_randomSeedQueue.Enqueue(_randomNumber);

        m_mapResourceCnt++;
    }

    [PunRPC]
    private void RPC_isCreateUpdate(int _x, int _y)
    {
        this.m_getMapInitialCreate.SetIsCreate(_x, _y, 1);
    }
    #endregion

    #region PublicMethod
    public void CheckExapandMapTrigger(float _playerPosX, float _playerPosY, bool _isAI)
    {
        m_isAI = _isAI;

        int mapPosX = GetMapPosition(_playerPosX);
        int mapPosY = GetMapPosition(_playerPosY);

        float centerArrayMapX = (mapPosX - m_mapArrayScale / 2) * (float)(m_mapSize);
        float centerArrayMapY = (mapPosY - m_mapArrayScale / 2) * (float)(m_mapSize);

        if (Mathf.Abs(_playerPosX - centerArrayMapX) > m_mapCreateTrigger)
            CheckXLineMap(_playerPosX, centerArrayMapX, mapPosX, mapPosY);

        if (Mathf.Abs(_playerPosY - centerArrayMapY) > m_mapCreateTrigger)
            CheckYLineMap(_playerPosY, centerArrayMapY, mapPosX, mapPosY);

        if (Mathf.Abs(_playerPosY - centerArrayMapY) > m_mapCreateTrigger && Mathf.Abs(_playerPosX - centerArrayMapX) > m_mapCreateTrigger)
            CheckXYLineMap(_playerPosX, centerArrayMapX, _playerPosY, centerArrayMapY, mapPosX, mapPosY);
    }

    public void SetMapChunk(List<Vector2> _posArray)
    {
        
        for(int arrayIndex = 0; arrayIndex < _posArray.Count; arrayIndex++)
        {
            int mapPosX = GetMapPosition(_posArray[arrayIndex].x);
            int mapPosY = GetMapPosition(_posArray[arrayIndex].y);

            if (mapPosX == m_curPlayerArrayX[arrayIndex] && mapPosY == m_curPlayerArrayY[arrayIndex])
                continue;

            m_curPlayerArrayX[arrayIndex] = mapPosX;
            m_curPlayerArrayY[arrayIndex] = mapPosY;

            for (int i = mapPosY - 2; i <= mapPosY + 2; i++) 
            {
                for(int j = mapPosX - 2; j <= mapPosX + 2; j++)
                {
                    if (m_mapChunkArray[i, j] == null)
                        continue;

                    if (i == mapPosY - 2 || i == mapPosY + 2)
                    {
                        if (OffMapChunk(_posArray, arrayIndex, j, i))
                            m_mapChunkArray[i, j].SetActive(false);
                    }

                    else if (j == mapPosX - 2 || j == mapPosX + 2)
                    {
                        if (OffMapChunk(_posArray, arrayIndex, j, i))
                            m_mapChunkArray[i, j].SetActive(false);
                    }

                    else
                        m_mapChunkArray[i, j].SetActive(true);
                }
            }

        }
    }

    private bool OffMapChunk(List<Vector2> _posArray, int _index, int _posX, int _posY)
    {
        for(int i = 0; i < _posArray.Count; i++)
        {
            if (i == _index)
                continue;

            for(int j = _posY - 1; j <= _posY + 1; j++)
            {
                for (int k = _posX - 1; k <= _posX + 1; k++)
                {
                    if (m_curPlayerArrayX[i] == k && m_curPlayerArrayY[i] == j)
                        return false;
                }
            }
        }

        return true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public void InitializeObject()
    {   
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                CreateMap(m_mapArrayScale / 2 + j, m_mapArrayScale / 2 + i);
            }
        }
        
    }

    public void StartGame()
    {
        
    }

    public void UpdateMapArray(GameObject _mapResource, int _arrayPosX, int _arrayPosY)
    {
        m_mapChunkArray[_arrayPosY, _arrayPosX] = _mapResource;
        if (m_isAI == true)
            return;

        if (Mathf.Abs(_arrayPosX - m_curPlayerArrayX[0]) >= 2 || Mathf.Abs(_arrayPosY - m_curPlayerArrayY[0]) >= 2)
            _mapResource.SetActive(false);
    }

    public void AddMapRousourceList(GameObject _mapResource)
    {
        m_MapResourceList.Add(_mapResource);
    }

    public void RemoveMineral(int _mapResourceIndex, int _mineralIndex)
    {
        m_photonView.RPC(nameof(RPC_MineralDestroy), RpcTarget.AllBuffered, _mapResourceIndex, _mineralIndex);
    }

    public void GameOverMapChunk(Vector3 _cartPos, Vector3 _playerPos)
    {
        int cartmapPosX = GetMapPosition(_cartPos.x);
        int cartmapPosY = GetMapPosition(_cartPos.y);
        int playermapPosX = GetMapPosition(_playerPos.x);
        int playermapPosY = GetMapPosition(_playerPos.y);

        SetActiveGameOverMap(playermapPosX, playermapPosY, cartmapPosX, cartmapPosY);
    }

    [PunRPC]
    public void RPC_MineralDestroy(int _mapResourceIndex, int _mineralIndex)
    {
        if (m_MapResourceList.Count > _mapResourceIndex)
        {
            m_MapResourceList[_mapResourceIndex].transform.GetChild(_mineralIndex).gameObject.SetActive(false);
            SoundManager.Instance.PlaySFXPos(SoundManager.SFX_GAIN_MINERAL, m_MapResourceList[_mapResourceIndex].transform.GetChild(_mineralIndex).transform.position, 0.3f);
        }
    }

    public void StopGame()
    {
        
    }
    #endregion
}

public interface IManageMineralArray
{
    void UpdateMapArray(GameObject _mapResource, int _arrayPosX, int _arrayPosY);
    public void AddMapRousourceList(GameObject _mapResource);
    public void RemoveMineral(int _mapResourceIndex, int _mineralIndex);

}

