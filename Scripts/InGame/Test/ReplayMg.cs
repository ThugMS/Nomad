using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ReplayMg : MonoBehaviour
{
     //제네릭 타입 <T>
    private bool recording = false;
    private bool replaying = false;

    /*
    public Action OnStartedRecording;
    public Action OnStoppedRecording;
    public Action OnStartedReplaying;
    public Action OnStoppedReplaying;
    */

    private MemoryStream memoryStream = null;
    private BinaryWriter binaryWriter = null;
    private BinaryReader binaryReader = null;

    private MemoryStream memoryStream2 = null;
    private BinaryWriter binaryWriter2 = null;
    private BinaryReader binaryReader2 = null;

    private bool recordingInitialized;

    public Transform[] transforms; //배우들
    public Transform[] transforms2;
    public List<Transform> transformList;

    public void Start()
    {
      //  transforms = FindObjectsOfType<Transform>(); //배우들 담기
        transformList = new List<Transform>(transforms);
    }
    public void Add(Transform _transform)
    {
       transformList.Add(_transform);
      //  transforms = transformList.ToArray();
    }

    public void FixedUpdate()
    {
        if (recording)
        {
            UpdateRecording();
        }
        else if (replaying)
        {
            UpdateReplaying();
        }
    }

    //왼쪽 버튼 연결
    public void StartStopRecording()
    {
        if (!recording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    private void InitializeRecording()
    {
        memoryStream = new MemoryStream();
        binaryWriter = new BinaryWriter(memoryStream);
        binaryReader = new BinaryReader(memoryStream);

        memoryStream2 = new MemoryStream();
        binaryWriter2 = new BinaryWriter(memoryStream2);
        binaryReader2 = new BinaryReader(memoryStream2);
        recordingInitialized = true;
    }


    private void StartRecording()
    {
        if (!recordingInitialized)
        {
            InitializeRecording();
        }
        else
        {
            memoryStream.SetLength(0);
        }

        ResetReplayFrame();

        recording = true;
        /*
        if (OnStartedRecording != null)
        {
            OnStartedRecording();
        }
        */
    }

    private void UpdateRecording()
    {
        SaveTransforms(transforms);
        SaveTransforms(transforms2);
        //  SaveInput();
    }

    private void StopRecording()
    {
        recording = false;
        /*
        if (OnStoppedRecording != null)
        {
            OnStoppedRecording();
        }
        */
    }

    private void ResetReplayFrame()
    {
        memoryStream.Seek(0, SeekOrigin.Begin);
        binaryWriter.Seek(0, SeekOrigin.Begin);

        memoryStream2.Seek(0, SeekOrigin.Begin);
        binaryWriter2.Seek(0, SeekOrigin.Begin);
    }

    //오른쪽 버튼 연결
    public void StartStopReplaying()
    {
        if (!replaying)
        {
            StartReplaying();
        }
        else
        {
            StopReplaying();
        }
    }

    private void StartReplaying()
    {
        ResetReplayFrame();

        replaying = true;
        /*
        if (OnStartedReplaying != null)
        {
            OnStartedReplaying();
        }
        */
    }

    private void UpdateReplaying()
    {
        if (memoryStream.Position >= memoryStream.Length)
        {
            StopReplaying();
            return;
        }
          LoadTransforms(transforms);
        LoadTransforms(transforms2);
        // LoadInput();
    }

    private void StopReplaying()
    {
        replaying = false;
        /*
        if (OnStoppedReplaying != null)
        {
            OnStoppedReplaying();
        }
        */
    }

    private void SaveTransforms(Transform[] transforms)
    {
        foreach (Transform transform in transforms)
        {
            SaveTransform(transform);
        }
    }

    private void SaveTransform(Transform transform)
    {
        //저장한 순서대로 위치, 방향, 크기를 분류해놓았으므로 순서를 지키는게 중요

        //제안 :포지션을 vector3 전체 값으로 저장이 가능한지 테스트해보기ㅣ.
        
        //포지션
        binaryWriter.Write(transform.localPosition.x);
        binaryWriter.Write(transform.localPosition.y);
        binaryWriter.Write(transform.localPosition.z);

        //몰루
        binaryWriter.Write(transform.localScale.x);
        binaryWriter.Write(transform.localScale.y);
        binaryWriter.Write(transform.localScale.z);

        binaryWriter.Write(transform.gameObject.activeSelf);

        //각도
        binaryWriter.Write(transform.localEulerAngles.x);
        binaryWriter.Write(transform.localEulerAngles.y);
        binaryWriter.Write(transform.localEulerAngles.z);

        // binaryWriter.Write("ㄹㄹ");
    }

    private void LoadTransforms(Transform[] transforms)
    {
        foreach (Transform transform in transforms)
        {
            LoadTransform(transform);
        }
    }

    private void LoadTransform(Transform transform)
    {
        float x = binaryReader.ReadSingle();
        float y = binaryReader.ReadSingle();
        float z = binaryReader.ReadSingle();
        transform.localPosition = new Vector3(x, y, z);

        x = binaryReader.ReadSingle();
        y = binaryReader.ReadSingle();
        z = binaryReader.ReadSingle();
        transform.localScale = new Vector3(x, y, z);

        bool on = binaryReader.ReadBoolean();
        transform.gameObject.SetActive(on);

        x = binaryReader.ReadSingle();
        y = binaryReader.ReadSingle();
        z = binaryReader.ReadSingle();
        transform.localEulerAngles = new Vector3(x, y, z);
    }
}
