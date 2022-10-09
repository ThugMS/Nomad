using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SoundManager : Singleton<SoundManager>
{
    public const string SFX_SHOOT1 = "sfx_shoot1";
    public const string SFX_SHOOT2 = "sfx_shoot2";
    public const string SFX_WARNING = "sfx_warning";
    public const string SFX_SHUU = "sfx_shuu";
    public const string SFX_NOMAD_DIE = "sfx_spaceHit";
    public const string SFX_NOMAD_EXPLOSION = "ship__explosion";
    public const string SFX_NOMAD_WEAPON1 = "ship_weapon_fire";
    public const string SFX_GAIN_MINERAL = "Mineral_Gain";
    public const string SFX_REPAIR = "repair";
    public const string SFX_PICKAXE = "sfx_pickaxe";
    public const string SFX_EXTRACTOR = "sfx_extractor";
    public const string SFX_PUT_MINERAL = "sfx_putmineral";

    public const string SFX_SYSTEM_MENU_CLICK = "sfx_click3";
    public const string SFX_SYSTEM_CLICK = "sfx_click5";
    public const string SFX_SYSTEM_DEL = "sfx_del";
    public const string SFX_SYSTEM_OPEN = "sfx_open";
    public const string SFX_SYSTEM_NOTI = "sfx_noti";
    public const string SFX_SYSTEM_UPGRADE = "sfx_upgrade";



    #region PrivateVariable
    [SerializeField] private AudioSource m_bgmSource;
    [SerializeField] private AudioClip[] m_sfxClips;

    [Header("각 씬마다 기본 BGM")]
    [Tooltip("씬이 바뀔때 씬의 기본 배경음을 설정, 기본 BGMClip은 씬의 이름과 같게 설정하기")]
    [SerializeField] private AudioClip[] m_defaultBgmClips;

    [Header("상황 BGM")]
    [SerializeField] private AudioClip[] m_extraBgmClips;

    private int m_sfxMaxSourceCount = 10;
    private List<AudioSource> m_sfxSources = new List<AudioSource>();
    private List<AudioSource> m_loopSfxSources = new List<AudioSource>();

    private Dictionary<string, AudioClip> m_sfxAudioClipDic = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> m_bgmAudioClipDic = new Dictionary<string, AudioClip>();

    private int m_lastSfxIndex = 0;
    private int m_lastLoopSfxIndex = 0;
    #endregion

    private void Awake()
    {
        if (m_instance != null)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        m_bgmSource.loop = true;
        
        for(int i  = 0; i < m_sfxMaxSourceCount; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            m_sfxSources.Add(source);

            AudioSource loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.loop = true;
            m_loopSfxSources.Add(loopSource);
        }

        for (int i = 0; i < m_sfxClips.Length; i++)
        {
            AudioClip clip = m_sfxClips[i];
            string name = clip.name;
            m_sfxAudioClipDic.Add(name, clip);
        }

        for (int i = 0; i < m_defaultBgmClips.Length; i++)
        {
            AudioClip clip = m_defaultBgmClips[i];
            string name = clip.name;
            m_bgmAudioClipDic.Add(name, clip);
        }
        for (int i = 0; i < m_extraBgmClips.Length; i++)
        {
            AudioClip clip = m_extraBgmClips[i];
            string name = clip.name;
            m_bgmAudioClipDic.Add(name, clip);
        }

        SceneManager.activeSceneChanged += ChangeBGM;
        PlayBGM(SceneManager.GetActiveScene().name);
    }

    private void ChangeBGM(Scene _prevScene, Scene _nextScene)
    {
        string name = _nextScene.name;
        PlayBGM(name);
    }

    #region PublicMethod
    public void PlaySFX(string _audioName, float _volume = 1)
    {
        if (!m_sfxAudioClipDic.ContainsKey(_audioName))
        {
            Debug.LogError($"{_audioName} 에 해당하는 효과음 클립이 없습니다. ");
            return;
        }
        int count = 0;
        for (int i = m_lastSfxIndex; ; i = (i + 1) % m_sfxMaxSourceCount)
        {
            count++;
            AudioSource source = m_sfxSources[i];
            if (source.isPlaying)
            {
                if (count == m_sfxMaxSourceCount)
                {
                    AudioSource audio = gameObject.AddComponent<AudioSource>();
                    m_sfxSources.Add(audio);
                    audio.PlayOneShot(m_sfxAudioClipDic[_audioName], _volume);
                    m_lastSfxIndex = m_sfxMaxSourceCount;
                }
                continue;
            }

            source.PlayOneShot(m_sfxAudioClipDic[_audioName], _volume);
            m_lastSfxIndex = i % m_sfxMaxSourceCount;
            return;
        }
    }
    public void PlaySFXPos(string _audioName, Vector2 _pos, float _volume = 1)
    {
        Vector2 viewPos = Camera.main.WorldToViewportPoint(_pos);
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
            return;

        PlaySFX(_audioName, _volume);
    }
    public void PlayLoopSFX(string _audioName, float _volume = 1, int _sourceIdx = 0)
    {
        if (!m_sfxAudioClipDic.ContainsKey(_audioName))
        {
            Debug.LogError($"{_audioName} 에 해당하는 효과음 클립이 없습니다. ");
            return;
        }

        AudioSource source = m_loopSfxSources[_sourceIdx];
        if (source.isPlaying)
            return;

        source.clip = m_sfxAudioClipDic[_audioName];
        source.volume = _volume;
        source.Play();
    }
    public void StopLoopSFX(int _sourceIdx = 0)
    {
        m_loopSfxSources[_sourceIdx].Stop();
    }
    public void PlayBGM(string _bgmName, float _volume = 0.4f)
    {
        if (!m_bgmAudioClipDic.ContainsKey(_bgmName))
        {
            Debug.LogError($"{m_bgmAudioClipDic} 에 해당하는 배경음 클립이 없습니다. ");
            return;
        }

        m_bgmSource.clip = m_bgmAudioClipDic[_bgmName];
        m_bgmSource.volume = _volume;
        m_bgmSource.Play();
    }
    public void ChagneSfxVolume(float _value)
    {
        for(int i = 0; i < m_sfxMaxSourceCount; i++)
        {
            m_sfxSources[i].volume = _value;
        }
    }
    public void ChangeBgmVolume(float _value)
    {
        m_bgmSource.volume = _value;
    }
    #endregion
}
