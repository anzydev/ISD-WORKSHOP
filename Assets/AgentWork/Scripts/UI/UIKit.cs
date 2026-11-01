using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CoinCatcher.UI
{
    public static class UIKit
    {
        public static Font PixelFont => Resources.Load<Font>("Fonts/PressStart2P");

        public static readonly Color Ink = new Color(0.055f, 0.04f, 0.11f, 0.92f);
        public static readonly Color Panel = new Color(0.09f, 0.065f, 0.18f, 0.97f);
        public static readonly Color NeonCyan = new Color(0.35f, 0.95f, 1f);
        public static readonly Color NeonPink = new Color(1f, 0.35f, 0.75f);
        public static readonly Color Gold = new Color(1f, 0.85f, 0.25f);
        public static readonly Color SoftWhite = new Color(0.92f, 0.9f, 1f);

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(go);
        }

        public static Canvas MakeCanvas(Transform parent, string name, int order = 0)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();
            return canvas;
        }

        public static RectTransform MakeRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Text MakeText(Transform parent, string name, string text, int size, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = PixelFont;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(-3f, 3f);
            return t;
        }

        public static Text Place(Text t, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = t.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return t;
        }

        public static RectTransform Fill(Transform parent, string name, Color color, float alpha = 1f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            var c = color;
            c.a *= alpha;
            img.color = c;
            return rt;
        }

        public static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, Color accent, UnityAction onClick, int fontSize = 26)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.11f, 0.08f, 0.21f, 0.98f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(accent.r, accent.g, accent.b, 0.55f);
            colors.pressedColor = new Color(accent.r * 0.7f, accent.g * 0.7f, accent.b * 0.7f, 1f);
            colors.fadeDuration = 0.06f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(onClick);

            var txt = MakeText(go.transform, "Label", label, fontSize, accent, TextAnchor.MiddleCenter);
            var txtRt = txt.rectTransform;
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(8f, 4f);
            txtRt.offsetMax = new Vector2(-8f, -4f);
            txt.raycastTarget = false;
            return btn;
        }

        public static Slider MakeSlider(Transform parent, Vector2 pos, Vector2 size, float value, Color accent, UnityAction<float> onChange)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var bg = new GameObject("BG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(1f, 0.5f);
            bgRt.sizeDelta = new Vector2(0f, 16f);
            bgRt.anchoredPosition = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.03f, 0.02f, 0.08f, 1f);

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fa = fillArea.GetComponent<RectTransform>();
            fa.anchorMin = new Vector2(0f, 0.5f);
            fa.anchorMax = new Vector2(1f, 0.5f);
            fa.sizeDelta = new Vector2(-24f, 12f);
            fa.anchoredPosition = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = accent;

            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var ha = handleArea.GetComponent<RectTransform>();
            ha.anchorMin = new Vector2(0f, 0.5f);
            ha.anchorMax = new Vector2(1f, 0.5f);
            ha.sizeDelta = new Vector2(-24f, 26f);
            ha.anchoredPosition = Vector2.zero;

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handRt = handle.GetComponent<RectTransform>();
            handRt.sizeDelta = new Vector2(26f, 26f);
            var handImg = handle.GetComponent<Image>();
            handImg.color = Color.white;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handRt;
            slider.targetGraphic = handImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            if (onChange != null) slider.onValueChanged.AddListener(onChange);
            return slider;
        }
    }
}
