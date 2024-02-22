using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Alchemy.UI
{
    public class AL_ScrollView:ScrollRect
    {
        public AL_UIPool UIPool;
        public int itemW;
        public int itemH;
        public int spacing;
        private IList _itemDatas;
        private bool _dataChange;

        public void SetData(IList list)
        {
            _itemDatas = list;
            _dataChange = true;
            
        }

        private void Update()
        {
            
        }

        private void RenderView()
        {
            if (_dataChange)
            {
                _dataChange = false;
            }
        }
    }

    public class AL_ScrollItem : MonoBehaviour
    {
        
    }
}