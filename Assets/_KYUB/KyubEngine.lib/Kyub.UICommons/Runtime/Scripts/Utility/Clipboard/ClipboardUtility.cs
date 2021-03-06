using UnityEngine;
using System.Runtime.InteropServices;
using System.Reflection;
using System;

namespace Kyub.UI
{
    public class ClipboardUtility
    {
        static IBoard _board;
        static IBoard board
        {
            get
            {
                if (_board == null)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    _board = new AndroidBoard();
#elif UNITY_IOS && !UNITY_EDITOR
                    _board = new IOSBoard ();
#elif UNITY_WEBGL && !UNITY_EDITOR
                    _board = new WebGLBoard ();
#else
                    _board = new StandardBoard();
#endif
                }
                return _board;
            }
        }

        public static void SetText(string str)
        {
            board.SetText(str);
        }

        public static string GetText()
        {
            return board.GetText();
        }
    }

    interface IBoard
    {
        void SetText(string str);
        string GetText();
    }

    class StandardBoard : IBoard
    {
        private static PropertyInfo m_systemCopyBufferProperty = null;
        private static PropertyInfo GetSystemCopyBufferProperty()
        {
            if (m_systemCopyBufferProperty == null)
            {
                Type T = typeof(GUIUtility);
                m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.Public);
                if (m_systemCopyBufferProperty == null)
                {
                    m_systemCopyBufferProperty =
                        T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
                }

                if (m_systemCopyBufferProperty == null)
                {
                    throw new Exception(
                        "Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
                }
            }
            return m_systemCopyBufferProperty;
        }
        public void SetText(string str)
        {
            PropertyInfo P = GetSystemCopyBufferProperty();
            P.SetValue(null, str, null);
        }
        public string GetText()
        {
            PropertyInfo P = GetSystemCopyBufferProperty();
            return (string)P.GetValue(null, null);
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    class WebGLBoard : IBoard {
        [DllImport("__Internal")]
        static extern void _JSSetToClipboard(string text);
        [DllImport("__Internal")]
        static extern string _JSGetFromClipboard();

        public void SetText(string str){
            GUIUtility.systemCopyBuffer = str;
            _JSSetToClipboard (str);
        }

        public string GetText(){
            var str = _JSGetFromClipboard();
            GUIUtility.systemCopyBuffer = str;
            return str;
        }
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    class IOSBoard : IBoard {
        [DllImport("__Internal")]
        static extern void SetText_ (string str);
        [DllImport("__Internal")]
        static extern string GetText_();

        public void SetText(string str){
            GUIUtility.systemCopyBuffer = str;
            if (Application.platform != RuntimePlatform.OSXEditor) {
                SetText_ (str);
            }
        }

        public string GetText(){
            var str = GetText_();
            GUIUtility.systemCopyBuffer = str;
            return str;
        }
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    class AndroidBoard : IBoard {

        AndroidJavaClass cb = new AndroidJavaClass("jp.ne.donuts.uniclipboard.Clipboard");

        public void SetText(string str){
            GUIUtility.systemCopyBuffer = str;
            cb.CallStatic ("setText", str);
        }

        public string GetText(){
            var str = cb.CallStatic<string> ("getText");
            GUIUtility.systemCopyBuffer = str;
            return str;
        }
    }
#endif
}
