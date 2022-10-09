using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolConstants
{
    public const float TOOL_GUN_RANGE = 10f;
    public const float TOOL_GUN_COOLTIME = 0.4f; //재사용하는데 필요한 대기 시간
    public const float TOOL_GUN_WORKTIME = 0; //동작을 완료하는데 필요한 작업 시간
    public const float TOOL_GUN_CAPABILITY = 35f;
        
    public const float TOOL_PICKAX_RANGE = 1f;
    public const float TOOL_PICKAX_COOLTIME = 0; 
    public const float TOOL_PICKAX_WORKTIME = 1.2f; 
    public const float TOOL_PICKAX_CAPABILITY = 1f;
       
    public const float TOOL_EXTRACTOR_RANGE = 1f;
    public const float TOOL_EXTRACTOR_COOLTIME = 0;
    public const float TOOL_EXTRACTOR_WORKTIME = 1.2f;
    public const float TOOL_EXTRACTOR_CAPABILITY = 1f;
           
    public const float TOOL_REPAIR_RANGE = 1f;
    public const float TOOL_REPAIR_COOLTIME = 0.02f;
    public const float TOOL_REPAIR_WORKTIME = 0;
    public const float TOOL_REPAIR_CAPABILITY = 3f;

}
