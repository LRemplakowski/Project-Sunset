using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Kalagaan.HairDesignerExtension
{
    [CustomEditor(typeof(HairDesignerMotionConstraintAreaBase),true)]
    public class HairDesignerMotionConstraintAreaEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            HairDesignerMotionConstraintAreaBase mca = target as HairDesignerMotionConstraintAreaBase;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Constraint", EditorStyles.boldLabel);
            mca.m_type = (MotionSolver.eMotionConstraint) EditorGUILayout.EnumPopup("Type", mca.m_type);
            GUILayout.EndVertical();


            if (Application.isPlaying)
                return;


            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Areas", EditorStyles.boldLabel);

            for (int i=0; i<mca.m_areas.Count; ++i)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.BeginVertical();
                mca.m_areas[i].position = EditorGUILayout.Vector3Field("Position", mca.m_areas[i].position);
                mca.m_areas[i].radius = EditorGUILayout.FloatField("Radius", mca.m_areas[i].radius);
                GUILayout.EndVertical();

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    mca.m_areas.RemoveAt(i--);
                    return;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();


            if ( GUILayout.Button("New area") )
            {
                mca.m_areas.Add(new HairDesignerMotionConstraintAreaBase.Area());
            }


        }



        public void OnSceneGUI()
        {
            if (Application.isPlaying)
                return;

            HairDesignerMotionConstraintAreaBase mca = target as HairDesignerMotionConstraintAreaBase;

            Vector3 r = Camera.current.transform.right;
            Vector3 camAxis = -Camera.current.transform.forward;

            for (int i = 0; i < mca.m_areas.Count; ++i)
            {
                Vector3 worldPos = mca.transform.TransformPoint(mca.m_areas[i].position);
                var fmh_73_67_638749745242912304 = Quaternion.identity; Vector3 newpos = Handles.FreeMoveHandle(worldPos, HandleUtility.GetHandleSize(worldPos) * .1f, Vector3.zero, Handles.CircleHandleCap);
                if (Vector3.Distance(worldPos, newpos) > .001f)
                {
                    mca.m_areas[i].position = mca.transform.InverseTransformPoint(newpos);
                    worldPos = mca.transform.TransformPoint(mca.m_areas[i].position);
                }

                Vector3 radius;


                radius = worldPos + r * mca.m_areas[i].radius * mca.transform.lossyScale.x;
                var fmh_84_57_638749745242930437 = Quaternion.identity; newpos = Handles.FreeMoveHandle(radius, HandleUtility.GetHandleSize(worldPos) * .05f, Vector3.zero, Handles.RectangleHandleCap);

                if (Vector3.Distance(radius, newpos) > .001f)
                    mca.m_areas[i].radius = Vector3.Distance(worldPos, newpos) / mca.transform.lossyScale.x;
                Handles.DrawDottedLine(worldPos, newpos, .2f);


                //Handles.SphereHandleCap(i, worldPos, Quaternion.identity, mca.m_areas[i].radius * mca.transform.lossyScale.x, EventType.MouseMove);
                Handles.DrawWireDisc(worldPos, camAxis, mca.m_areas[i].radius * mca.transform.lossyScale.x);
                /*
                if (i == 0)
                {
                    Vector3 w = mca.transform.TransformPoint(mca.m_areas[i].position);
                    

                    if(mca.m_areas.Count>1)
                    {
                        Vector3 p = w + r * mca.m_areas[i].radius * mca.transform.lossyScale.x;
                        //Handles.DrawWireArc(w, -Camera.current.transform.forward, )
                    }
                    else
                    {
                        
                    }                    
                }*/


                if ( i>0 )
                {



                    Vector3 w1 = mca.transform.TransformPoint(mca.m_areas[i-1].position);
                    Vector3 w2 = mca.transform.TransformPoint(mca.m_areas[i].position);
                    Handles.DrawDottedLine(w1, w2, .2f);

                    Vector3 axis = w2 - w1;
                    Vector3 dir = Vector3.Cross(Camera.current.transform.forward, axis).normalized;


                    Vector3 p1 = w1 + dir * mca.m_areas[i-1].radius * mca.transform.lossyScale.x;
                    Vector3 p2 = w2 + dir * mca.m_areas[i].radius * mca.transform.lossyScale.x;
                    Handles.DrawLine(p1, p2);

                    p1 = w1 - dir * mca.m_areas[i - 1].radius * mca.transform.lossyScale.x;
                    p2 = w2 - dir * mca.m_areas[i].radius * mca.transform.lossyScale.x;
                    Handles.DrawLine(p1, p2);
                }
            }

        }
    }
}