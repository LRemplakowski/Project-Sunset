using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kalagaan
{
    namespace HairDesignerExtension
    {

        [CustomEditor(typeof(HairDesignerGenerator), true)]
        public class HairGeneratorEditor : Editor
        {

            public bool guiChanged = false;
            public bool hideWireframe = false;
            //public EditorSelectedRenderState renderState = EditorSelectedRenderState.Highlight;
            public HairDesignerEditor m_hairDesignerEditor = null;
            public Editor m_shaderEditor;


            public static int m_currentEditorLayer = 0;

            //brush parameters
            protected float m_brushFalloff = .5f;            
            protected float m_brushIntensity = .5f;
            protected float m_brushScale = 1f;
            protected float m_brushSpacing = 1f;
            protected bool m_CtrlMode = false;
            protected bool m_ShiftMode = false;
            protected bool m_AltMode = false;
            protected float m_brushSize = .1f;
            protected float m_brushRadius = .1f;
            protected float m_constUnit = 1;
            protected float m_brushDensity = .1f;
            protected float m_pixelRange = 1f;
            protected float m_brushRaise = .1f;
            protected int m_brushAtlasID = -1;
            protected bool m_brushRandomAtlasId = true;

#if UNITY_5_6_OR_NEWER
            protected Handles.CapFunction RectangleCap = Handles.RectangleHandleCap;
            protected Handles.CapFunction CircleCap = Handles.CircleHandleCap;
            protected Handles.CapFunction SphereCap = Handles.SphereHandleCap;
            protected Handles.CapFunction ArrowCap = Handles.ArrowHandleCap;
            protected Handles.CapFunction ConeCap = Handles.ConeHandleCap;
            protected Handles.CapFunction DotCap = Handles.DotHandleCap;
#else
            protected Handles.DrawCapFunction RectangleCap = Handles.RectangleCap;
            protected Handles.DrawCapFunction CircleCap = Handles.CircleCap;
            protected Handles.DrawCapFunction SphereCap = Handles.SphereCap;
            protected Handles.DrawCapFunction ArrowCap = Handles.ArrowCap;
#endif




            public override void OnInspectorGUI()
            {
                
                GUI.color = Color.white;
                HairDesignerGenerator g = target as HairDesignerGenerator;
                               

                if (g == null) return;
                


                if (g.m_hd == null)
                {
                    GUILayout.Label("Runtime Layer can't be edited", EditorStyles.helpBox);
                    return;
                }




#if UNITY_2018_3_OR_NEWER
                if (PrefabUtility.IsPartOfPrefabAsset(g))
#else
                if (PrefabUtility.GetPrefabParent(g.gameObject) == null && PrefabUtility.GetPrefabObject(g.gameObject) != null)
#endif
                {
                    GUILayout.Label("Instantiate the prefab for modifications", EditorStyles.helpBox);
                    return;
                }


                bool isMeshReadable = true;
                if (g.m_hd.m_smr != null && !g.m_hd.m_smr.sharedMesh.isReadable)
                    isMeshReadable = false;
                if (g.m_hd.m_mf != null && !g.m_hd.m_mf.sharedMesh.isReadable)
                    isMeshReadable = false;

                if (!isMeshReadable)
                {
                    EditorGUILayout.HelpBox("The mesh is not readable\nPlease enable the Read/Write option in the fbx settings", MessageType.Error);
                    return;
                }



                if (g.m_hd == null)
                {
                    if (g.m_meshInstance != null)
                    {
                        DestroyImmediate(g.m_meshInstance);
                        g.m_meshInstance = null;
                    }

                    DestroyImmediate(g);                                        
                    return;
                }



                m_currentEditorLayer = Mathf.Clamp(m_currentEditorLayer, 0, g.m_editorLayers.Count-1);

                //GUILayout.Label("Name", GUILayout.MaxWidth(100));
                //GUILayout.Space(10);
                //GUILayout.Label("Edit layer", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(-10);
                GUILayout.Label(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.SEPARATOR));
                GUILayout.Space(-6);

                GUILayout.BeginHorizontal(HairDesignerEditor.bgStyle);
                g.m_name = EditorGUILayout.TextField(new GUIContent("Layer", (HairDesignerEditor.Icon(g.m_meshLocked ? HairDesignerEditor.eIcons.LOCKED: HairDesignerEditor.eIcons.UNLOCKED))), g.m_name, GUILayout.MinHeight(25));



                if (!Application.isPlaying)
                {
                    if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.SAVE), GUILayout.MaxWidth(25)))
                    {
                        HairDesignerEditorUtility.ExportRuntimeLayer(g);
                    }
                }



                if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.DUPLICATE), GUILayout.MaxWidth(25)))
                {
                    if (EditorUtility.DisplayDialog("Duplicate layer", "Duplicate layer '" + g.m_name + "' ?", "Ok", "Cancel"))
                    {
                        Undo.RecordObject(g.m_hd, "Duplicate layer");
                        HairDesignerGenerator src = g.m_hd.generator;
                        HairDesignerGenerator dest = g.m_hd.gameObject.AddComponent(src.GetType()) as HairDesignerGenerator;
                        dest.hideFlags = HideFlags.HideInInspector;
                        
                        EditorUtility.CopySerialized(src, dest);
                        g.m_hd.m_generators.Add(dest);
                        dest.m_name += "(copy)";
                        //destroy link to existing mesh
                        dest.m_meshInstance = null;
                        dest.m_hair = null;
                        dest.GenerateMeshRenderer();
                        dest.m_meshLocked = true;
                        /*
                        if ( dest.m_layerType == HairDesignerBase.eLayerType.FUR_SHELL  )
                            dest.m_meshLocked = false;
                        if (dest.m_layerType == HairDesignerBase.eLayerType.FUR_GEOMETRY)
                            dest.m_meshLocked = false;
                            */
                        for (int i = 0; i < dest.m_shaderParams.Count; ++i)
                        {
                            dest.m_shaderParams[i] = g.m_hd.gameObject.AddComponent(dest.m_shaderParams[i].GetType() ) as HairDesignerShader;
                            dest.m_shaderParams[i].hideFlags = HideFlags.HideInInspector;
                            EditorUtility.CopySerialized(src.m_shaderParams[i], dest.m_shaderParams[i]);                             
                        }

                        g.GenerateMeshRenderer();
                        OnDuplicate( dest );
                    }
                }
                


                if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.TRASH), GUILayout.MaxWidth(25)))
                {
                    if (EditorUtility.DisplayDialog("Delete layer", "Delete layer '"+g.m_name+"' ?", "Ok", "Cancel"))
                    {
                        Undo.RecordObject(g.m_hd, "Remove layer");

                        g._editorDelete = true;
                        if (g.m_meshInstance != null)
                        {
                            if (Application.isPlaying)
                                Destroy(g.m_meshInstance);
                            else
                                DestroyImmediate(g.m_meshInstance);
                        }


                        for (int j = 0; j < g.m_shaderParams.Count; ++j)
                        {
                            if (Application.isPlaying)
                                Destroy(g.m_shaderParams[j]);
                            else
                                DestroyImmediate(g.m_shaderParams[j]);
                        }
                        g.m_shaderParams.Clear();

                        HairDesignerGenerator component = g.m_hd.generator;
                        g.m_hd.m_generators.Remove(g.m_hd.generator);                        
                        g.m_hd.m_generatorId = -1;
                        g.m_hd = null;
                        if (!Application.isPlaying)
                            g.Destroy();


                        if ( Application.isPlaying )
                            Destroy(component);
                        else
                            DestroyImmediate(component);
                    }
                };
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                guiChanged = GUI.changed;

            }



            public virtual void OnDuplicate( HairDesignerGenerator copy )
            {



  
            }

            /// <summary>
            /// Draw T-Pose mode switch button
            /// </summary>
            /// <param name="hd"></param>
            public void TPoseModeChoose(HairDesignerBase hd)
            {
                if (hd.m_tpose != null)
                {
                    GUILayout.Label(new GUIContent("Tpose", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);
                    //EditorGUILayout.HelpBox("if the TPose is not applied, change the mode",MessageType.Info);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    HairDesignerGenerator g = target as HairDesignerGenerator;

                    bool changeTpose = false;
                    TPoseUtility.eTPoseMode mode = TPoseUtility.eTPoseMode.NONE;

                    /*
                    if (GUILayout.Button("None" ))
                    {
                        mode = TPoseUtility.eTPoseMode.NONE;
                        changeTpose = true;
                    }*/
                    if (GUILayout.Button("Mode 1"))
                    {                        
                        mode = TPoseUtility.eTPoseMode.Mode_1;
                        changeTpose = true;
                    }
                    if (GUILayout.Button("Mode 2"))
                    {                       
                        mode = TPoseUtility.eTPoseMode.Mode_2;
                        changeTpose = true;
                    }

                    if(changeTpose)
                    {
                        hd.m_tpose.RevertTpose();
                        hd.m_tpose.m_TPoseMode = mode;
                        m_hairDesignerEditor.RemoveCollider();
                        m_hairDesignerEditor.CreateColliderAndTpose(g.m_skinningVersion160);
                        //hd.m_tpose.ApplyTPose(hd.GetComponent<SkinnedMeshRenderer>(), false);

                    }

                    /*
                    if (GUILayout.Button("switch to mode" + (hd.m_tpose.m_TPoseMode == TPoseUtility.eTPoseMode.Mode_1 ? "2" : "1")))
                    {
                        hd.m_tpose.m_TPoseMode = hd.m_tpose.m_TPoseMode == TPoseUtility.eTPoseMode.Mode_1 ? TPoseUtility.eTPoseMode.Mode_2 : TPoseUtility.eTPoseMode.Mode_1;
                        hd.m_tpose.RevertTpose();
                        hd.m_tpose.ApplyTPose(hd.GetComponent<SkinnedMeshRenderer>(), false);
                    }*/
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }


            /// <summary>
            /// Draw Material settings
            /// </summary>
            public void DrawMaterialSettings()
            {
                HairDesignerGenerator g = target as HairDesignerGenerator;
                if (g.m_hd == null)
                    return;

                GUILayout.Label(new GUIContent("Material", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                //Material old = g.m_hairMeshMaterial;
                g.m_hairMeshMaterial = EditorGUILayout.ObjectField("Material", g.m_hairMeshMaterial, typeof(Material), true) as Material;

                //if (g.m_hairMeshMaterial != old)
                //    g.DestroyMesh();//mesh will be regenerated with new material

                if (g.m_hairMeshMaterial == null)
                    return;
                
                string shaderName = g.m_hairMeshMaterial.shader.name;
                GUILayout.Label("Shader : "  + shaderName);
                GUILayout.Space(10f);

                GUILayout.Label(new GUIContent("Instance parameters", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);


                /*
                if (g.m_matPropBlkHair == null)
                    return;
                    */


                    if ( !g.m_shaderParams.Exists(sp => sp.m_shader != null && sp.m_shader.name == g.m_hairMeshMaterial.shader.name ) )
                {
                    string shaderNameWithoutpath = g.m_hairMeshMaterial.shader.name.Split('/').Last();

                    Object[] script = Resources.FindObjectsOfTypeAll(typeof(MonoScript))
                        .Where(x => x.name == "HairDesignerShader" + shaderNameWithoutpath)
                        //.Where(x => x.name.Contains("HairDesignerShader"))
                        .ToArray();

                    if (script.Length > 0)
                    {
                        //Debug.Log("scripts found : " + script.Length);
                        //for ( int i=0; i< script.Length; ++i  )
                        {
                            //ScriptableObject s = ScriptableObject.CreateInstance((script[0] as MonoScript).GetClass());                                                      
                            //(s as HairDesignerShader).m_shader = g.m_hairMeshMaterial.shader;
                            System.Type t = (script[0] as MonoScript).GetClass();
                            HairDesignerShader s = g.m_hd.gameObject.AddComponent(t) as HairDesignerShader;
                            s.hideFlags = HideFlags.HideInInspector;
                            s.m_shader = g.m_hairMeshMaterial.shader;

                            if (s != null)
                            {
                                g.m_shaderParams.Add(s);
                                s.m_generator = g;
                            }
                            else
                                Debug.LogWarning("Shader param instance failed");
                        }
                    }
                    else
                    {
                        g.m_hairMeshMaterialTransparent = null;//disable the transparent pass
                        if (g.m_hairMeshMaterial.shader.name.Split('/').First() != "HairDesigner")
                        {
                            EditorGUILayout.HelpBox("This shader is not supported by Hair Designer\nCreate a 'HairDesignerShader' script for using per instance parameters.", MessageType.Info);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("There's no instance parameters available for this shader.", MessageType.Info);
                        }
                        
                        if( GUILayout.Button("Open material settings") )
                        {
                            Selection.activeObject = g.m_hairMeshMaterial;
                        }                      
                    }
                }
                else
                {                   

                    HairDesignerShader hds = g.m_shaderParams.FindLast(sp => sp.m_shader == g.m_hairMeshMaterial.shader);
                    hds.m_generator = g;
                    //hds.InitPropertyBlock(ref g.m_matPropBlkHair, g.m_layerType);
                    m_shaderEditor = Editor.CreateEditor( hds );
                    m_shaderEditor.OnInspectorGUI();
                    //hds.UpdatePropertyBlock(ref g.m_matPropBlkHair);

                }


            }


            public virtual void DrawSceneTools()
            {                

            }


            public virtual void DrawSceneMenu(float width)
            {
                GUILayout.Label("DRAW SCENE MENU");

            }




            public virtual void PaintToolAction()
            {

            }



            /// <summary>
            /// Draw brush
            /// </summary>
            /// <param name="mp"></param>
            /// <param name="svCam"></param>
            public virtual void DrawBrush()
            {
                HairDesignerBase hd = (target as HairDesignerGenerator).m_hd;
                Event e = Event.current;

                Camera svCam = SceneView.currentDrawingSceneView.camera;
                svCam.nearClipPlane = .01f;
                Vector2 mp2d = e.mousePosition * EditorGUIUtility.pixelsPerPoint;//retina display has 2 pixels per point
                mp2d.y = svCam.pixelHeight - mp2d.y;

                Vector3 mp = Vector3.zero;
                
                if (svCam.orthographic)
                {
                    mp = svCam.ScreenToWorldPoint(mp2d);
                    mp += svCam.transform.forward;
                    m_constUnit = (svCam.ViewportToWorldPoint(Vector3.zero) - svCam.ViewportToWorldPoint(Vector3.one)).magnitude;
                    m_constUnit = HandleUtility.GetHandleSize(hd.transform.position) * 10f;
                    m_brushSize = m_brushRadius * m_constUnit;
                }
                else
                {
                    Ray r = svCam.ScreenPointToRay(mp2d);
                    mp = svCam.transform.position + r.direction.normalized;
                    m_brushSize = m_brushRadius;
                }



                Color c = m_CtrlMode ? Color.red : Color.blue;
                c = m_ShiftMode ? Color.yellow : c;

                Handles.color = c;
                Handles.BeginGUI();
                //GUILayout.BeginArea(new Rect(mp.x * (float)svCam.pixelWidth, (float)svCam.pixelHeight- mp.y * (float)svCam.pixelHeight*.5f, 200, 50));
                Vector2 posScreen = svCam.WorldToScreenPoint(mp + svCam.transform.up * m_brushSize);
                GUI.color = c;
                GUILayout.BeginArea(new Rect(posScreen.x, svCam.pixelHeight - posScreen.y - 20, 200, 50));

                //GUILayout.Label("brush");
                GUILayout.EndArea();
                Handles.EndGUI();


                c.a = .05f + .1f * m_brushIntensity;
                Handles.color = c;
                //Handles.DrawSolidDisc(mp, -svCam.transform.forward, m_brushSize);

                float gradient = 10f;

                for (float i = 0; i < gradient; ++i)
                {
                    c.a = .1f * m_brushIntensity * i / gradient;
                    Handles.color = c;

                    float f = (1f - (m_brushFalloff * i) / gradient);
                    Handles.DrawSolidDisc(mp, -svCam.transform.forward, m_brushSize * f);
                }

                c.a = .03f * m_brushIntensity;
                Handles.color = c;
                Handles.DrawSolidDisc(mp, -svCam.transform.forward, m_brushSize * (1f - (m_brushFalloff)));

                //c = Color.blue;
                c.a = 1f;
                Handles.color = c;
                Handles.DrawWireDisc(mp, -svCam.transform.forward, m_brushSize);

                c.a = .1f;
                Handles.color = c;
                Handles.DrawWireDisc(mp, -svCam.transform.forward, m_brushSize * (1f - m_brushFalloff));


            }




            string[] _boneNames = null;

            /// <summary>
            /// Select bone popup
            /// </summary>
            /// <param name="selected"></param>
            /// <returns></returns>
            public Transform SelectBone(Transform selected)
            {
                //return selected;
                HairDesignerGenerator g = target as HairDesignerGenerator;
                SkinnedMeshRenderer smr = g.m_hd.m_smr;//.GetComponent<SkinnedMeshRenderer>();

                if (smr != null )
                {
                    int id = 0;

                    
                    if (_boneNames == null)
                    {
                        _boneNames = new string[smr.bones.Length + 1];
                        _boneNames[0] = "- none -";
                        for (int i = 0; i < smr.bones.Length; ++i)                        
                            _boneNames[i + 1] = smr.bones[i].name;
                    }

                    
                    for (int i = 0; i < smr.bones.Length; ++i)
                    {
                        if (smr.bones[i] == selected)
                        {
                            id = i + 1;
                            break;
                        }
                    }
                                        
                    int newid = EditorGUILayout.Popup("Parent Bone", id, _boneNames);

                    if (newid == id)
                        return null;

                    if (newid == 0)
                        return g.m_hd.transform;
                    else
                        return smr.bones[newid - 1];
                }
                else
                    return null;
            }






           


            public virtual void DeleteEditorLayer(  int idx )
            {
                if (idx == m_currentEditorLayer)
                    m_currentEditorLayer = -1;
                else if (idx < m_currentEditorLayer)
                    m_currentEditorLayer--;
            }






            /// <summary>
            /// MotionZoneSceneGUI
            /// </summary>
            protected void MotionZoneSceneGUI()
            {
                HairDesignerBase hd = (target as HairDesignerGenerator).m_hd;
                Camera SvCam = SceneView.currentDrawingSceneView.camera;

                if (hd.generator == null)
                    return;


                for (int i = 0; i < hd.generator.m_motionZones.Count; ++i)
                {
                    Vector3 handlePos = hd.generator.m_motionZones[i].parent.TransformPoint(hd.generator.m_motionZones[i].localPosition);

                    Color c = new Color(0, 0, 1f, .1f);
                    Handles.color = c;

                    Handles.DrawSolidDisc(handlePos, -SvCam.transform.forward, hd.generator.m_motionZones[i].radius * hd.globalScale * (SvCam.orthographic ? 1f : 1.07f));
                    c.r = 1f;
                    c.g = 1f;
                    c.a = .3f;
                    Handles.color = c;



                    Handles.DrawWireDisc(handlePos, hd.generator.m_motionZones[i].parent.up, hd.generator.m_motionZones[i].radius * hd.globalScale);
                    Handles.DrawWireDisc(handlePos, hd.generator.m_motionZones[i].parent.forward, hd.generator.m_motionZones[i].radius * hd.globalScale);
                    Handles.DrawWireDisc(handlePos, hd.generator.m_motionZones[i].parent.right, hd.generator.m_motionZones[i].radius * hd.globalScale);

                    //Vector3 newPos = Handles.FreeMoveHandle(hd.m_motionZones[i].parent.TransformPoint(hd.m_motionZones[i].localPosition), Quaternion.identity, hd.m_motionZones[i].radius, Vector3.zero, Handles.CircleCap);

                    var fmh_599_72_638749745244216267 = Quaternion.identity; Vector3 newPos = Handles.FreeMoveHandle(handlePos, HandleUtility.GetHandleSize(handlePos) * .5f, Vector3.zero, SphereCap);
                    hd.generator.m_motionZones[i].localPosition = hd.generator.m_motionZones[i].parent.InverseTransformPoint(newPos);

                    c = Color.yellow;
                    Handles.color = c;
                    Handles.DrawLine(hd.generator.m_motionZones[i].parent.position, handlePos);

                    Handles.Label(hd.generator.m_motionZones[i].parent.position, hd.generator.m_motionZones[i].parent.gameObject.name, EditorStyles.centeredGreyMiniLabel);

                    var fmh_608_158_638749745244227109 = Quaternion.identity; Vector3 radiusHandle = Handles.FreeMoveHandle(handlePos + SvCam.transform.right * hd.generator.m_motionZones[i].radius * hd.globalScale, HandleUtility.GetHandleSize(handlePos) * .2f, Vector3.zero, CircleCap);
                    if (hd.globalScale > 0)
                        hd.generator.m_motionZones[i].radius = (radiusHandle - handlePos).magnitude / hd.globalScale;
                    else
                        hd.generator.m_motionZones[i].radius = 0f;
                }
            }


#if UNITY_5_6_OR_NEWER            
            public Vector3 FreeMoveHandleTransformPoint(Transform t, Vector3 position, Quaternion rotation, float size, Vector3 snap, Handles.CapFunction capFunc)
            {                
                Vector3 newPos = t.InverseTransformPoint( Handles.FreeMoveHandle(t.TransformPoint( position ), size, snap, capFunc));
#else
            public Vector3 FreeMoveHandleTransformPoint(Transform t, Vector3 position, Quaternion rotation, float size, Vector3 snap, Handles.DrawCapFunction capFunc)
            {
                Vector3 newPos = t.InverseTransformPoint(Handles.FreeMoveHandle(t.TransformPoint(position), rotation, size, snap, capFunc));
#endif



                //avoid unwanted micro move in scene view
                if (Vector3.Distance(newPos, position) > .000001f)
                {
                    //Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    return newPos;
                }

                //

                return position;
            }




            public void DrawLayerPanel()
            {

                HairDesignerGenerator g = target as HairDesignerGenerator;

                if (g.m_meshLocked)
                    return;

                float width = 60;
                float Xpos = SceneView.currentDrawingSceneView.camera.pixelWidth- width;
                float Ypos = 120;
                

                Handles.BeginGUI();
                GUILayout.BeginArea(new Rect(Xpos, Ypos, width, SceneView.currentDrawingSceneView.camera.pixelHeight));
                GUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.MaxWidth(width));
                
                GUILayout.Label("Layers", EditorStyles.centeredGreyMiniLabel);

                if (  g.m_editorLayers.Count == 0)
                    g.m_editorLayers.Add(new HairToolLayer());


                int deleteID = -1;

                for (int i = 0; i < g.m_editorLayers.Count; ++i)
                {   
                    GUILayout.BeginHorizontal();
                    GUI.color = m_currentEditorLayer == i ? Color.white : Color.gray;
                    if (GUILayout.Button("" + (i + 1), EditorStyles.miniButtonLeft, GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        if (Event.current.button == 1)
                        {
                            //right clic : delete                            
                            deleteID = i;                        
                        }
                        else
                        {
                            //left clic : select
                            m_currentEditorLayer = i;
                            /*
                            if (!g.m_editorLayers[m_currentEditorLayer].visible)
                            {
                                g.m_editorLayers[m_currentEditorLayer].visible = true;
                                OnLayerChange();
                            }*/
                        }
                        
                    }

                    //GUILayout.FlexibleSpace();
                    if (GUILayout.Button(HairDesignerEditor.Icon(g.m_editorLayers[i].visible ? HairDesignerEditor.eIcons.VISIBLE : HairDesignerEditor.eIcons.HIDDEN), EditorStyles.miniButtonRight, GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        Undo.RecordObject(g, "hairDesigner layer visibility");
                        g.m_editorLayers[i].visible = !g.m_editorLayers[i].visible;
                        OnLayerChange();
                    }
                    GUILayout.EndHorizontal();
                }
                GUI.color = Color.white;


                if (GUILayout.Button("+", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(20)))
                {
                    Undo.RecordObject(g, "Add new layer");//save generator for undo
                    g.m_editorLayers.Add(new HairToolLayer());
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            
                GUILayout.EndArea();

                Handles.EndGUI();

                if (deleteID != -1)
                {
                    DeleteEditorLayer(deleteID);
                    OnLayerChange();
                }
            }



            public virtual void OnLayerChange()
            {

            }


            public static Tool m_lastTool;
            public void DisableSceneTool()
            {


#if UNITY_2019_1_OR_NEWER
                if (Tools.current != Tool.Custom)
                    m_lastTool = Tools.current;
#if UNITY_2020_2_OR_NEWER                
                UnityEditor.EditorTools.ToolManager.SetActiveTool(typeof(HairDesignerEditorTool));
#else
                UnityEditor.EditorTools.EditorTools.SetActiveTool(typeof(HairDesignerEditorTool));
#endif

#else
                if(Tools.current != Tool.None)
                    m_lastTool = Tools.current;
                Tools.current = Tool.None;
#endif

                HairDesignerEditor.EnableSceneViewGizmos();
            }

            public void EnableSceneTool()
            {

#if UNITY_2019_1_OR_NEWER

                //if (Tools.current == Tool.None && m_lastTool!= Tool.None)
                if (Tools.current == Tool.Custom && m_lastTool != Tool.None)
                    Tools.current = m_lastTool;
#else
                if (Tools.current == Tool.None && m_lastTool!= Tool.None)
                    Tools.current = m_lastTool;
#endif
            }



            float[] m_blendShapesWeights = null;

            /// <summary>
            /// Check blendshape modification
            /// </summary>
            /// <returns></returns>
            public bool CheckBlendshapesModifications()
            {
                HairDesignerGenerator g = target as HairDesignerGenerator;

                if ( g==null || g.m_hd.m_smr == null || g.m_hd.m_smr.sharedMesh == null || g.m_hd.m_smr.sharedMesh.blendShapeCount == 0)
                    return false;

                if (m_blendShapesWeights == null || m_blendShapesWeights.Length != g.m_hd.m_smr.sharedMesh.blendShapeCount)
                {
                    m_blendShapesWeights = new float[g.m_hd.m_smr.sharedMesh.blendShapeCount];
                    for(int i=0; i< g.m_hd.m_smr.sharedMesh.blendShapeCount; ++i)
                    {
                        m_blendShapesWeights[i] = g.m_hd.m_smr.GetBlendShapeWeight(i);
                    }
                    return false;
                }

                bool r = false;
                for (int i = 0; i < g.m_hd.m_smr.sharedMesh.blendShapeCount; ++i)
                {
                    if( m_blendShapesWeights[i] != g.m_hd.m_smr.GetBlendShapeWeight(i))
                        r=true;
                    m_blendShapesWeights[i] = g.m_hd.m_smr.GetBlendShapeWeight(i);
                }

                return r;
            }



            public void EditorGUILayoutListField( Object o, string propertyName )
            {
                if (o == null)
                    return;

                var serializedObject = new SerializedObject(o);
                if (serializedObject != null)
                {
                    var property = serializedObject.FindProperty(propertyName);
                    if (property != null)
                    {
                        serializedObject.Update();
                        EditorGUILayout.PropertyField(property, true);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }




            public int Toolbar(int selected,GUIContent[] contents)
            {
                //return GUILayout.Toolbar(selected, contents);
                int r = selected;
                GUILayout.BeginHorizontal();
                GUIStyle s = EditorStyles.miniButton;
                for (int i = 0; i < contents.Length; ++i)
                {
                    GUI.color = selected == i ? Color.white : Color.gray;
                    s = i == 0 ? EditorStyles.miniButtonLeft : (i == contents.Length - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid);
                    if (GUILayout.Button(contents[i], s))
                        r = i;
                }
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
                return r;
            }


            public int Toolbar(int selected, string[] contents)
            {
                //return GUILayout.Toolbar(selected, contents);
                int r = selected;
                GUILayout.BeginHorizontal();                
                GUIStyle s = EditorStyles.miniButton;
                for ( int i=0; i< contents.Length; ++i )
                {
                    GUI.color = selected == i ? Color.white : Color.gray;
                    s = i==0? EditorStyles.miniButtonLeft : (i== contents.Length-1 ? EditorStyles.miniButtonRight : EditorStyles.miniButton) ;
                    if (GUILayout.Button(contents[i], s))
                        r = i;
                }
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
                return r;
            }



            
        }
    }
}