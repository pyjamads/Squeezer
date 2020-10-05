using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof (AdjustableRangeAttribute))]
    public class AdjustableRangeDrawer : PropertyDrawer
    {
        private bool initialized = false;
        private string minId;
        private string maxId;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            AdjustableRangeAttribute attribute = (AdjustableRangeAttribute) this.attribute;
            
            if (initialized == false)
            {
                var instanceId = property.serializedObject.targetObject.GetInstanceID().ToString();
                if(!attribute.lockMin)
                {
                    minId = instanceId + property.name + "min";
                    if (EditorPrefs.HasKey(minId))
                    {
                        attribute.min = EditorPrefs.GetFloat(minId);
                    }
                }
                if(!attribute.lockMax)
                {
                    maxId = instanceId + property.name + "max";
                    if (EditorPrefs.HasKey(maxId))
                    {
                        attribute.max = EditorPrefs.GetFloat(maxId);
                    }
                }
                initialized = true;
            }
            
            var minMaxWidth = 80f;
            var minMaxLabelWidth = 55f;
            
            var indentedRect = EditorGUI.IndentedRect(position);

            var widthOffset = position.width - indentedRect.width;            
            
            var labelPos = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            
            var minLabelPos = new Rect(position.x + labelPos.width - minMaxLabelWidth, position.y, minMaxLabelWidth, position.height);
            var minPos = new Rect(position.x + labelPos.width - widthOffset, position.y, minMaxWidth, position.height);
            
            var sliderPos = new Rect(minPos.x + minPos.width - widthOffset + EditorGUIUtility.standardVerticalSpacing, position.y, position.width - labelPos.width - 2f*minMaxWidth + 4f*widthOffset - 2f*EditorGUIUtility.standardVerticalSpacing - minMaxLabelWidth, position.height);
            
            var maxLabelPos = new Rect(sliderPos.x + sliderPos.width - widthOffset + EditorGUIUtility.standardVerticalSpacing, position.y, minMaxLabelWidth, position.height);
            var maxPos = new Rect(maxLabelPos.x + maxLabelPos.width - widthOffset, position.y, minMaxWidth, position.height);

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.LabelField(labelPos, label);

                if (!attribute.lockMin)
                {
                    EditorGUI.LabelField(minLabelPos, "min");
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        //Clamp min value to the clampMin, and the current property value.  
                        attribute.min = Mathf.Max(attribute.clampMin,
                            Mathf.Min(property.floatValue,
                                EditorGUI.FloatField(minPos, GUIContent.none, attribute.min)));

                        if (check.changed)
                        {
                            EditorPrefs.SetFloat(minId, attribute.min);
                        }
                    }
                }
                else
                {
                    sliderPos.x -= minPos.width - widthOffset;
                    sliderPos.width += minPos.width - widthOffset;
                }

                if(!attribute.lockMax)
                {
                    EditorGUI.LabelField(maxLabelPos, "max");
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        //Clamp min value to the clampMax, and the current property value.
                        attribute.max = Mathf.Min(attribute.clampMax,
                            Mathf.Max(property.floatValue,
                                EditorGUI.FloatField(maxPos, GUIContent.none, attribute.max)));

                        if (check.changed)
                        {
                            EditorPrefs.SetFloat(maxId, attribute.max);
                        }
                    }
                }
                else
                {
                    sliderPos.width += maxPos.width - EditorGUIUtility.standardVerticalSpacing;
                }
                
                EditorGUI.Slider(sliderPos, property, attribute.min, attribute.max, GUIContent.none);
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(labelPos, label);
                EditorGUI.LabelField(minLabelPos, "min");
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    attribute.min = Mathf.Max(attribute.clampMin, Mathf.Min(property.intValue,EditorGUI.IntField(minPos, GUIContent.none, (int)attribute.min)));

                    if (check.changed)
                    {
                        EditorPrefs.SetFloat(minId, attribute.min);
                    }
                }

                EditorGUI.IntSlider(sliderPos, property, (int)attribute.min, (int)attribute.max, GUIContent.none);

                EditorGUI.LabelField(maxLabelPos, "max");
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    attribute.max = Mathf.Min(attribute.clampMax, Mathf.Max(property.intValue, EditorGUI.IntField(maxPos, GUIContent.none, (int) attribute.max)));

                    if (check.changed)
                    {
                        EditorPrefs.SetFloat(maxId, attribute.max);
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
            }
        }
    }
}