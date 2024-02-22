using System.Collections.Generic;

namespace Alchemy
{
    public class AL_NoteInfo
    {
        private static AL_NoteInfo _instance;
        public static AL_NoteInfo Instance => _instance ??= new AL_NoteInfo();
        
        private Dictionary<string, List<string>> _noteDic = new Dictionary<string, List<string>>();

        public AL_NoteInfo()
        {
            AL_EventMgr.Add<AlGamePreQuit>(this, OnGameQuit);
        }

        private void OnGameQuit(AlGamePreQuit quit)
        {
            foreach (var kv in _noteDic)
            {
                AL_FileLog.LogError($"[noteInfo Log] key={kv.Key};  val={string.Join(",", kv.Value)}");
            }
        }

        public void AddNote(string key, string val)
        {
            if (!_noteDic.ContainsKey(key))
            {
                _noteDic.Add(key, new List<string>() {val});
            }
            else
            {
                if (_noteDic.TryGetValue(key, out var tempList) && !tempList.Contains(val))
                {
                    tempList.Add(val);
                }
            }
        }

        public void AddNote(string key, List<string> vals)
        {
            foreach (var val in vals)
            {
                AddNote(key, val);
            }
        }
    }
}