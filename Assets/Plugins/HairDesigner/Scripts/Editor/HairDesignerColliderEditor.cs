using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kalagaan.HairDesignerExtension
{    
    [CustomEditor(typeof(HairDesignerColliderBase),true)]
    public class HairDesignerColliderEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            HairDesignerColliderBase col = target as HairDesignerColliderBase;

            
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Sphere 1", EditorStyles.miniBoldLabel);
            if(col.m_sc != null || col.m_cc != null)
            {
                col.m_useUnityColliderSettings1 = EditorGUILayout.Toggle("Use Collider settings", col.m_useUnityColliderSettings1);
            }
            else
            {
                col.m_useUnityColliderSettings1 = false;
            }


            if (!col.m_useUnityColliderSettings1)
            {
                col.m_dualSphereInternalData.center1 = EditorGUILayout.Vector3Field("Center", col.m_dualSphereInternalData.center1);
                col.m_dualSphereInternalData.radius1 = EditorGUILayout.FloatField("Radius", col.m_dualSphereInternalData.radius1);
                col.m_transform1 = EditorGUILayout.ObjectField("Transform", col.m_transform1, typeof(Transform), true) as Transform;
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Sphere 2", EditorStyles.miniBoldLabel);


            if (col.m_sc != null || col.m_cc != null)
            {
                col.m_useUnityColliderSettings2 = EditorGUILayout.Toggle("Use Collider settings", col.m_useUnityColliderSettings2);
            }
            else
            {
                col.m_useUnityColliderSettings2 = false;
            }


            if (!col.m_useUnityColliderSettings2)
            {
                col.m_dualSphereInternalData.center2 = EditorGUILayout.Vector3Field("Center", col.m_dualSphereInternalData.center2);
                col.m_dualSphereInternalData.radius2 = EditorGUILayout.FloatField("Radius", col.m_dualSphereInternalData.radius2);
                col.m_transform2 = EditorGUILayout.ObjectField("Transform", col.m_transform2, typeof(Transform), true) as Transform;
            }


            GUILayout.EndVertical();

            if (GUI.changed)
                col.UpdateData();

            //GUILayout.Space(20);
            //base.OnInspectorGUI();
        }



        public void OnSceneGUI()
        {
            HairDesignerColliderBase col = target as HairDesignerColliderBase;


            Vector3 center1 = col.m_dualSphereWorldData.center1;
            Vector3 center2 = col.m_dualSphereWorldData.center2;
            Vector3 r = Camera.current.transform.right;
            Vector3 radius;

            Handles.color = Color.cyan;
            Handles.DrawDottedLine(center1, center2, 2f);


            if (!col.m_useUnityColliderSettings1)
            {
                Handles.color = Color.cyan;
                var fmh_88_70_638749745242912091 = Quaternion.identity; Vector3 newcenter1 = Handles.FreeMoveHandle(center1, HandleUtility.GetHandleSize(center1) * .1f, Vector3.zero, Handles.CircleHandleCap);
                if (Vector3.Distance(newcenter1, center1) > .01f)
                {
                    center1 = newcenter1;
                    col.SetWorldCenter(center1, 1);
                }
                radius = center1 + r * col.m_dualSphereWorldData.radius1;
                var fmh_95_57_638749745242930437 = Quaternion.identity; radius = Handles.FreeMoveHandle(radius, HandleUtility.GetHandleSize(center1) * .1f, Vector3.zero, Handles.RectangleHandleCap);
                col.SetWorldRadius(Vector3.Distance(center1, radius), .001f, 1);
                Handles.DrawDottedLine(center1, radius, .2f);
            }
            else
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(center1, Camera.current.transform.forward, HandleUtility.GetHandleSize(center1) * .1f);
            }


            if (!col.m_useUnityColliderSettings2)
            {
                Handles.color = Color.cyan;
                var fmh_109_70_638749745242939240 = Quaternion.identity; Vector3 newcenter2 = Handles.FreeMoveHandle(center2, HandleUtility.GetHandleSize(center2) * .1f, Vector3.zero, Handles.CircleHandleCap);
                if (Vector3.Distance(newcenter2, center2) > .01f)
                {
                    center2 = newcenter2;
                    col.SetWorldCenter(center2, 2);
                }
                radius = center2 + r * col.m_dualSphereWorldData.radius2;
                var fmh_116_57_638749745242945940 = Quaternion.identity; radius = Handles.FreeMoveHandle(radius, HandleUtility.GetHandleSize(center2) * .1f, Vector3.zero, Handles.RectangleHandleCap);
                col.SetWorldRadius(Vector3.Distance(center2, radius), .001f, 2);
                Handles.DrawDottedLine(center2, radius, .2f);
            }
            else
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(center2, Camera.current.transform.forward, HandleUtility.GetHandleSize(center2) * .1f);
            }

            col.UpdateData();
        }
    }

}