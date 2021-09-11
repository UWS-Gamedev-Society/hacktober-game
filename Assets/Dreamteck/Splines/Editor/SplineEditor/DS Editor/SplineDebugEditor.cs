namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class SplineDebugEditor : SplineEditorBase
    {
        SplineComputer spline;
        float length = 0f;

        private SerializedObject serializedObject;
        private SerializedProperty editorPathColor;
        private SerializedProperty editorAlwaysDraw;
        private SerializedProperty editorDrawThickness;
        private SerializedProperty editorBillboardThickness;
        private SerializedProperty editorUpdateMode;

        public SplineDebugEditor(SplineComputer spline, SerializedObject serializedObject) : base()
        {
            this.spline = spline;
            this.serializedObject = serializedObject;
            GetSplineLength();
            editorPathColor = serializedObject.FindProperty("editorPathColor");
            editorAlwaysDraw = serializedObject.FindProperty("editorAlwaysDraw");
            editorDrawThickness = serializedObject.FindProperty("editorDrawThickness");
            editorBillboardThickness = serializedObject.FindProperty("editorBillboardThickness");
            editorUpdateMode = serializedObject.FindProperty("editorUpdateMode");
        }

        void GetSplineLength()
        {
            length = Mathf.RoundToInt(spline.CalculateLength() * 100f) / 100f;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (Event.current.type == EventType.MouseUp) GetSplineLength();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(editorUpdateMode, new GUIContent("Editor Update Mode"));
            EditorGUILayout.PropertyField(editorPathColor, new GUIContent("Color in Scene"));
            bool lastAlwaysDraw = editorAlwaysDraw.boolValue;
            EditorGUILayout.PropertyField(editorAlwaysDraw, new GUIContent("Always Draw Spline"));
            if (lastAlwaysDraw != editorAlwaysDraw.boolValue)
            {
                if (editorAlwaysDraw.boolValue)
                {
                    for (int i = 0; i < serializedObject.targetObjects.Length; i++)
                    {
                        if (serializedObject.targetObjects[i] is SplineComputer)
                        {
                            DSSplineDrawer.RegisterComputer((SplineComputer)serializedObject.targetObjects[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < serializedObject.targetObjects.Length; i++)
                    {
                        if (serializedObject.targetObjects[i] is SplineComputer)
                        {
                            DSSplineDrawer.UnregisterComputer((SplineComputer)serializedObject.targetObjects[i]);
                        }
                    }
                }
            }
            EditorGUILayout.PropertyField(editorDrawThickness, new GUIContent("Draw thickness"));
            if (editorDrawThickness.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(editorBillboardThickness, new GUIContent("Always face camera"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            if (serializedObject.targetObjects.Length == 1)
            {
                EditorGUILayout.HelpBox("Samples: " + spline.samples.Length + "\n\r" + "Length: " + length, MessageType.Info);
            } else
            {
                EditorGUILayout.HelpBox("Multiple spline objects selected" + length, MessageType.Info);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (editorUpdateMode.intValue == 0)
                {
                    for (int i = 0; i < serializedObject.targetObjects.Length; i++)
                    {
                        if(serializedObject.targetObjects[i] is SplineComputer)
                        {
                            ((SplineComputer)serializedObject.targetObjects[i]).RebuildImmediate(true);
                        }
                    }
                    SceneView.RepaintAll();
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void DrawScene(SceneView current)
        {
            base.DrawScene(current);
            if (Event.current.type == EventType.MouseUp && open)
            {
                GetSplineLength();
            }
        }
    }
}
