/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;

namespace Alchemy
{
    public class AL_EventStruct
    {
        
    }
    
    public struct AlGamePreQuit
    {
        public bool Succ;
    }
    public struct AlCrossDay
    {
        public DateTime DateTime;
    }
    
    //整分、整点触发
    public struct AlMinuteChange{}
    public struct AlHourChange{}
    
    //按照秒、分、时、循环触发
    public struct AlSecLoop{}
    public struct AlMinLoop{}
    public struct AlHourLoop{}
    
    //每次同步/校准时间触发
    public struct AlSyncTime{}
}