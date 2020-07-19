using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace dninosores.UnityAlphaCorrector
{
    /// <summary>
    /// Allows for setting a UI image's alpha to values greater than 1 by duplicating the object.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class AlphaCorrector : MonoBehaviour
    {
        public float Alpha
        {
            get
            {
                return alpha;
            }

            set
            {
                alpha = value;
                OnValidate();
            }
        }

        [SerializeField, Range(0, 10)]
        protected float alpha = 1;

        [SerializeField, HideInInspector]
        private List<Image> duplicates = new List<Image>();

        public virtual void OnValidate()
        {
            if (alpha < 0)
            {
                alpha = 0;
            }
            SetAlpha();
        }


        [ContextMenu("Clear")]
        public void Clear()
        {
            duplicates.Clear();
        }

        [ContextMenu("Update")]
        public void SetAlpha()
        {
            if (duplicates == null)
            {
                duplicates = new List<Image>();
            }

            if (alpha >= 0)
            {
                Image rootImage = GetComponent<Image>();
                Color rootColor = rootImage.color;
                float tempAlpha = alpha;

                if (tempAlpha <= 1.0)
                {
                    rootColor.a = tempAlpha;
                    rootImage.color = rootColor;
                    for (int i = 0; i < duplicates.Count; i++)
                    {
                        SafeDestroy(duplicates[i].gameObject);
                        duplicates.RemoveAt(i);
                        i--;
                    }
                    return;
                }

                tempAlpha -= 1;
                rootColor.a = 1;
                rootImage.color = rootColor;

                for (int i = 0; i < duplicates.Count; i++)
                {
                    if (tempAlpha >= 1)
                    {
                        duplicates[i].color = rootColor;
                        tempAlpha -= 1;
                    }
                    else if (tempAlpha > 0)
                    {
                        rootColor.a = tempAlpha;
                        duplicates[i].color = rootColor;
                        tempAlpha = 0;
                    }
                    else
                    {
                        GameObject obj = duplicates[i].gameObject;
                        duplicates.RemoveAt(i);
                        i--;
                        SafeDestroy(obj);
                    }
                }

                while (tempAlpha > 0)
                {
                    GameObject obj = new GameObject("AlphaObject");
                    obj.transform.SetParent(transform, false);
                    obj.transform.SetAsFirstSibling();
                    RectTransform rt = obj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.offsetMin = new Vector2(0, 0);
                    rt.offsetMax = new Vector2(0, 0);
                    Image newImage = CloneImage(rootImage, obj);
                    duplicates.Add(newImage);

                    if (tempAlpha >= 1)
                    {
                        newImage.color = rootColor;
                        tempAlpha -= 1;
                    }
                    else if (tempAlpha > 0)
                    {
                        rootColor.a = tempAlpha;
                        newImage.color = rootColor;
                        tempAlpha = 0;
                    }
                }

            }
        }

        [ContextMenu("CleanUpdate")]
        public void Clean()
        {
            Clear();
            SetAlpha();
        }

        Image CloneImage(Image original, GameObject destination)
        {
            Image i = destination.AddComponent<Image>();
            i.sprite = original.sprite;
            i.color = original.color;
            i.material = original.material;
            i.type = original.type;
            i.raycastTarget = false;
            i.preserveAspect = original.preserveAspect;
            i.GetComponent<RectTransform>().pivot = GetComponent<RectTransform>().pivot;
            return i;
        }

        private void SafeDestroy(UnityEngine.Object go)
        {
            if (go == null)
            {
                return;
            }
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(go, true);
                };
            else
#endif
                Destroy(go);
        }
    }
}
