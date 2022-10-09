static class MapConstants
{
    public const int BEFORE_SEARCH = 0;
    public const int FIND_STARLIGHT = 1;
    public const int FIND_CAZELIN = 2;
    public const int CREATE_TRIGGER_SIZE = 15;
    public const int SPAWN_AREA = 4;
    public const float COMPARE_CONSTANT = 0.001f;
    public const float OFFSET_RANGE = 10000f;
    public const float VARIABLE_RANGE = 1f;
    public const float STARLIGHT_PROBABILITY = 1 / 5f;
    public const float CAZELIN_PROBABILITY = 2 / 5f;
    public const float CART_PROBABLITY = 0.01f;
    public const float RESOROUCE_SIZE = 1.5f;
}

public class MineralPosition
{
    #region PrivateVariable
    private int m_randomSeed = 1;
    private float m_starlightLevel = 0.1f;
    private float m_cazelinLevel = 0.2f;
    private float m_stoneLevel = 0.35f;
    private float m_noiseScale = 0.1f;
    private int m_mapArrayScale = 1000;
    private int m_mapSize = 20;
    private int m_mapArrayPos = 30;
    #endregion

    #region PublicVariable
    public bool isStarlight = false;
    public bool isCazelin = false;
    public bool isBlock = false;
    public bool isCart = false;
    #endregion

    #region PublicMethod
    public int GetRandomSeed()
    {
        return m_randomSeed;
    }
    public float GetStarlightLevel()
    {
        return m_starlightLevel;
    }
    public float GetCazelinLevel()
    {
        return m_cazelinLevel;
    }
    public float GetStoneLevel()
    {
        return m_stoneLevel;
    }
    public float GetNoiseScale()
    {
        return m_noiseScale;
    }
    public int GetMapArrayScale()
    {
        return m_mapArrayScale;
    }
    public int GetMapSize()
    {
        return m_mapSize;
    }
    public int GetMapArrayPos()
    {
        return m_mapArrayPos;
    }
    #endregion
}
