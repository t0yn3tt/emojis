﻿#if UNITY_2018_2_OR_NEWER
#define TMP_WEBGL_SUPPORT
#endif

#if TMP_WEBGL_SUPPORT
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MobileInputNativePlugin.WebGL.Detail;

namespace MobileInputNativePlugin.WebGL
{
    /// <summary>
    /// Wrapper for TMPro.TMP_InputField
    /// </summary>
    class WrappedTMPInputField : IInputField
    {
        TMP_InputField input;
        RebuildChecker checker;

        public bool ReadOnly { get { return input.readOnly; } }

        public string text
        {
            get { return input.text; }
            set { input.text = value; }
        }

        public int fontSize
        {
            get { return (int)input.textComponent.fontSize; }
        }

        public ContentType contentType
        {
            get { return (ContentType)input.contentType; }
        }

        public LineType lineType
        {
            get { return (LineType)input.lineType; }
        }

        public int characterLimit
        {
            get { return input.characterLimit; }
        }

        public int caretPosition
        {
            get { return input.caretPosition; }
        }

        public bool isFocused
        {
            get { return input.isFocused; }
        }

        public int selectionFocusPosition
        {
            get { return input.selectionStringFocusPosition; }
            set { input.selectionStringFocusPosition = value; }
        }

        public int selectionAnchorPosition
        {
            get { return input.selectionStringAnchorPosition; }
            set { input.selectionStringAnchorPosition = value; }
        }

        public bool OnFocusSelectAll
        {
            get { return input.onFocusSelectAll; }
        }

        public WrappedTMPInputField(TMP_InputField input)
        {
            this.input = input;
            checker = new RebuildChecker(this);
        }

        public RectTransform RectTransform()
        {
            // 表示範囲
            // MEMO :
            //  TMP では textComponent を移動させてクリッピングするため、
            //  表示範囲外になる場合があるので、自分の範囲を返す
            return input.GetComponent<RectTransform>();
        }

        public void ActivateInputField()
        {
            input.ActivateInputField();
        }

        public void DeactivateInputField()
        {
            input.DeactivateInputField();
        }

        public void Rebuild()
        {
            if (input.textComponent.enabled && checker.NeedRebuild())
            {
                //================================
                // fix bug for tmp
                // TMPの不具合で、正しく座標を設定されてなかったため、試しに対応する
                var rt = input.textComponent.GetComponent<RectTransform>();
                var size = input.textComponent.GetPreferredValues();
                if (size.x < rt.rect.xMax)
                {
                    // textComponent の座標を更新
                    var pos = rt.anchoredPosition;
                    pos.x = 0;
                    rt.anchoredPosition = pos;

                    // caret の座標更新
                    var caret = input.GetComponentInChildren<TMP_SelectionCaret>();
                    var caretRect = caret.GetComponent<RectTransform>();
                    caretRect.anchoredPosition = rt.anchoredPosition;
                }
                //==============================

                // HACK : 1フレーム無効にする
                // MEMO : 他にいい方法Rebuildがあれば対応する
                // LayoutRebuilder.ForceRebuildLayoutImmediate(); で試してダメでした
                input.textComponent.enabled = false;
                input.Rebuild(CanvasUpdate.LatePreRender);
                input.textComponent.SetAllDirty();
            }
            else
            {
                input.textComponent.enabled = true;
            }
        }
    }

}

#endif // TMP_WEBGL_SUPPORT
