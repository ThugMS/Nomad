using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;

public class MapInitialCreate : MonoBehaviour
{
    #region PrivateVariables
    [SerializeField] private GameObject m_mapResourceParent;
    [SerializeField] private GameObject m_starlight;
    [SerializeField] private GameObject m_cazelin;
    [SerializeField] private GameObject m_stone_v1;
    [SerializeField] private GameObject m_stone_v2;
    [SerializeField] private GameObject m_stone_v3;
    [SerializeField] private GameObject m_background;
    [SerializeField] private GameObject m_starlightCart;
    [SerializeField] private GameObject m_cazelinCart;
    [SerializeField] private GameObject m_weaponCart;


    private IManageMineralArray m_manageMineral;

    private int[] m_dx = { 1, -1, 0, 0 };
    private int[] m_dy = { 0, 0, 1, -1 };

    private MineralPosition m_mapVariables = new MineralPosition();
    private GameObject m_getResourcesParent;
    private MineralPosition[,] m_grid;
    private int m_resourceStar = MapConstants.BEFORE_SEARCH;

    private int m_randomSeed;
    private int m_mapSize;
    private int m_mapArrayScale;
    private int m_mapResourceParentCnt;
    private float m_starlightLevel;
    private float m_cazelinLevel;
    private float m_stoneLevel;
    private float m_scale;
    #endregion

    #region PublicPublicVariables
    public int[,] m_isCreate;
    #endregion

    #region PrivateMethod
    protected void Awake()
    { 
        Random.InitState((int)m_mapVariables.GetRandomSeed());

        m_mapArrayScale = m_mapVariables.GetMapArrayScale();

        m_mapSize = m_mapVariables.GetMapSize();
        m_starlightLevel = m_mapVariables.GetStarlightLevel();
        m_cazelinLevel = m_mapVariables.GetCazelinLevel();
        m_stoneLevel = m_mapVariables.GetStoneLevel();
        m_scale = m_mapVariables.GetNoiseScale();
        m_randomSeed = m_mapVariables.GetRandomSeed();

        m_isCreate = new int[m_mapArrayScale, m_mapArrayScale];
        m_isCreate[m_mapArrayScale / 2, m_mapArrayScale / 2] = 1;

        m_starlight.transform.localScale = new Vector3(MapConstants.RESOROUCE_SIZE, MapConstants.RESOROUCE_SIZE, MapConstants.RESOROUCE_SIZE);
        m_cazelin.transform.localScale = new Vector3(MapConstants.RESOROUCE_SIZE, MapConstants.RESOROUCE_SIZE, MapConstants.RESOROUCE_SIZE);
        m_stone_v1.transform.localScale = new Vector3(MapConstants.RESOROUCE_SIZE, MapConstants.RESOROUCE_SIZE, MapConstants.RESOROUCE_SIZE);
        m_background.transform.localScale = new Vector3(m_mapSize * MapConstants.RESOROUCE_SIZE, m_mapSize * MapConstants.RESOROUCE_SIZE, 1);
    }

    private void FindMergeResource(float[,] _noiseMap, bool[,] _findMap, int _x, int _y)
    {
        if (_x >= m_mapSize || _x < 0 || _y >= m_mapSize || _y < 0)
            return;

        if (_noiseMap[_x, _y] >= m_stoneLevel)
            return;

        if (_findMap[_x, _y] == true)
            return;

        _findMap[_x, _y] = true;

        if (_noiseMap[_x, _y] < m_cazelinLevel)
        {
            if (m_resourceStar == MapConstants.BEFORE_SEARCH)
            {
                if (_noiseMap[_x, _y] >= m_starlightLevel)
                    m_resourceStar = MapConstants.FIND_STARLIGHT;

                else
                    m_resourceStar = MapConstants.FIND_CAZELIN;
            }
            else if (m_resourceStar == MapConstants.FIND_STARLIGHT)
                _noiseMap[_x, _y] = m_starlightLevel + MapConstants.COMPARE_CONSTANT;

            else
                _noiseMap[_x, _y] = m_starlightLevel - MapConstants.COMPARE_CONSTANT;
        }
    
        for (int i = 0; i < m_dx.Length; i++)
            FindMergeResource(_noiseMap, _findMap, _x + m_dx[i], _y + m_dy[i]);
    }

    private void CreateMap(int _mapPosX, int _mapPosY)
    {
        int mineralCnt = 0;
        for (int y = 0; y < m_mapSize; y++)
        {
            for (int x = 0; x < m_mapSize; x++)
            {
                Vector3 pos = new Vector3((x + (_mapPosX * m_mapSize) - (m_mapSize / 2))*MapConstants.RESOROUCE_SIZE, (y + (_mapPosY * m_mapSize) - (m_mapSize / 2))* MapConstants.RESOROUCE_SIZE, 0);
                MineralPosition cell = m_grid[x, y];
                GameObject mineral = null;
                float probability = Random.Range(0f, 100f);
                if (cell.isStarlight)
                {
                    mineral = Instantiate(m_starlight, pos, Quaternion.identity);
                    SetMineralIndex(mineral, mineralCnt);
                }
                else if (cell.isCazelin)
                {
                    mineral = Instantiate(m_cazelin, pos, Quaternion.identity);
                    SetMineralIndex(mineral, mineralCnt);
                }
                else if (cell.isBlock)
                {
                    if (probability > 66f)
                    {
                        mineral = Instantiate(m_stone_v1, pos, Quaternion.identity);
                    }
                    else if (probability > 33f)
                    {
                        mineral = Instantiate(m_stone_v2, pos, Quaternion.identity);
                    }
                    else
                    {
                        mineral = Instantiate(m_stone_v3, pos, Quaternion.identity);
                    }
                    
                    SetMineralIndex(mineral, mineralCnt);
                }
                else
                {
                    GameObject cart = null;
                    if (probability > (100f - (MapConstants.CART_PROBABLITY * 3)))
                    {   
                        if(PhotonNetwork.IsMasterClient)
                            cart = PhotonNetwork.Instantiate(ConstStringStorage.NOMAD_WEAPONCART_PATH, pos, Quaternion.identity);    
                    }    
                    else if (probability > (100f - (MapConstants.CART_PROBABLITY * 4)))
                    {
                        if (PhotonNetwork.IsMasterClient)
                            cart = PhotonNetwork.Instantiate(ConstStringStorage.NOMAD_CAZELINCART_PATH, pos, Quaternion.identity);
                    }
                    else if (probability > (100f - (MapConstants.CART_PROBABLITY * 5)))
                    {
                        if (PhotonNetwork.IsMasterClient)
                            cart = PhotonNetwork.Instantiate(ConstStringStorage.NOMAD_STARLIGHT_PATH, pos, Quaternion.identity);
                    }  
                }
                if (mineral != null)
                {
                    mineral.transform.parent = m_getResourcesParent.transform;
                    mineralCnt++;
                }   
            }
        }

        //함선 무기칸 테스트 코드
        //PhotonNetwork.Instantiate(ConstStringStorage.NOMAD_WEAPONCART_PATH, new Vector3(10,0,0), Quaternion.identity);
        //PhotonNetwork.Instantiate(ConstStringStorage.NOMAD_WEAPONCART_PATH, new Vector3(10, 0, 0), Quaternion.identity);

        Vector3 pos1 = new Vector3(_mapPosX * m_mapSize * MapConstants.RESOROUCE_SIZE, _mapPosY * m_mapSize * MapConstants.RESOROUCE_SIZE, 0);
        GameObject background = Instantiate(m_background, pos1, Quaternion.identity);
        background.transform.parent = m_getResourcesParent.transform;

        m_manageMineral.UpdateMapArray(m_getResourcesParent, _mapPosX + (m_mapArrayScale / 2), _mapPosY + (m_mapArrayScale / 2));

        m_manageMineral.AddMapRousourceList(m_getResourcesParent);
    }

    private void SetMineralIndex(GameObject _mineral, int _mineralCnt)
    {
        _mineral.GetComponent<MineralBase>().SetIndex(m_mapResourceParentCnt, _mineralCnt);
        _mineral.GetComponent<MineralBase>().OnFinished += m_manageMineral.RemoveMineral;
        _mineral.GetComponent<MineralBase>().m_onFinishedAction += m_manageMineral.RemoveMineral;
    }

    private bool IsInitialMap(int _mapPosX, int _mapPosY)
    {
        if (_mapPosX == 0 && _mapPosY == 0)
            return true;
        return false;
    }

    private bool IsSpawnArea(int _y)
    {
        return (_y >= m_mapSize / 2 - MapConstants.SPAWN_AREA && _y <= m_mapSize / 2 + MapConstants.SPAWN_AREA);
    }
    #endregion

    #region PublicMethod
    public void NoiseCreateMap(int _mapPosX, int _mapPosY, int _randomSeedNumber)
    {            
        this.m_getResourcesParent = Instantiate(m_mapResourceParent, Vector3.zero, Quaternion.identity);

        m_randomSeed = _randomSeedNumber;
        Random.InitState(m_randomSeed);

        float[,] noiseMap = new float[m_mapSize, m_mapSize];
        bool[,] findMap = new bool[m_mapSize, m_mapSize];
        float xOffset = Random.Range(-MapConstants.OFFSET_RANGE, MapConstants.OFFSET_RANGE);
        float yOffset = Random.Range(-MapConstants.OFFSET_RANGE, MapConstants.OFFSET_RANGE);

        for (int y = 0; y < m_mapSize; y++)
        {
            for (int x = 0; x < m_mapSize; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * m_scale + xOffset, y * m_scale + yOffset);
                noiseMap[x, y] = noiseValue;

                if (noiseValue < m_stoneLevel && noiseValue >= m_cazelinLevel)
                {
                    float sub = Random.Range(0.0f, MapConstants.VARIABLE_RANGE);

                    if (sub < MapConstants.STARLIGHT_PROBABILITY)
                        noiseMap[x, y] = m_starlightLevel - MapConstants.COMPARE_CONSTANT;

                    else if (sub < MapConstants.CAZELIN_PROBABILITY)
                        noiseMap[x, y] = m_starlightLevel + MapConstants.COMPARE_CONSTANT;
                }
            }
        }
        for (int y = 0; y < m_mapSize; y++)
        {
            for (int x = 0; x < m_mapSize; x++)
            {
                if (noiseMap[x, y] < m_stoneLevel && findMap[x, y] != true)
                {
                    FindMergeResource(noiseMap, findMap, x, y);
                    m_resourceStar = MapConstants.BEFORE_SEARCH;
                }

            }
        }
        m_grid = new MineralPosition[m_mapSize, m_mapSize];

        for (int y = 0; y < m_mapSize; y++)
        {
            for (int x = 0; x < m_mapSize; x++)
            {
                MineralPosition cell = new MineralPosition();
                float noiseValue = noiseMap[x, y];

                if (IsInitialMap(_mapPosX, _mapPosY))
                {
                    if (IsSpawnArea(y))
                    {
                        m_grid[x, y] = cell;
                        continue;
                    }
                }
                if (noiseValue < m_starlightLevel)
                    cell.isStarlight = true;
                else if (noiseValue < m_cazelinLevel)
                    cell.isCazelin = true;
                else if (noiseValue < m_stoneLevel)
                {
                    float sub = Random.Range(0.0f, MapConstants.VARIABLE_RANGE);
                    cell.isBlock = true;
                }
                m_grid[x, y] = cell;
            }
        }
        CreateMap(_mapPosX, _mapPosY);
    }
    public void CreateInitialMap()
    {
        NoiseCreateMap(0, 0, m_randomSeed);
    }

    public int GetIsCreate(int _x, int _y)
    {
        return m_isCreate[_x, _y];
    }

    public void SetIsCreate(int _x, int _y, int value)
    {
        m_isCreate[_x, _y] = value;
    }

    public void SetMapResourceCnt(int _mapResourceCnt)
    {
        m_mapResourceParentCnt = _mapResourceCnt;
    }

    public void AllocateInterface(IManageMineralArray _manageMineral)
    {
        m_manageMineral = _manageMineral;
    }
    #endregion

}
