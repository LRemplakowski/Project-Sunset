#define HAIRDESIGNER_MOTIONSYSTEM_V3

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kalagaan
{
    namespace HairDesignerExtension
    {

        [CustomEditor(typeof(HairDesignerGeneratorLongHairBase), true)]
        public class HairGeneratorLongHairBaseEditor : HairGeneratorEditor
        {


            //GUIStyle bgStyle = null;

            public enum eTab
            {
                //INFO,
                DESIGN,
                MATERIAL,
                MOTION,
                ADVANCED,
                CURVE = 10,
                MIRROR,
                DEBUG = 100
            }
            public static eTab m_tab = eTab.MATERIAL;
            public static eTab m_SceneTab = eTab.CURVE;
            public int m_subToolId = 0;

            Vector3 m_lastHairPos = Vector3.zero;
            ePaintingTool m_currentTool = ePaintingTool.ADD;

            static bool m_showSelectionButtons = false;
            //static bool m_showSelectionOnly = true;
            static bool m_changeSeedOnDuplicate = true;
            static Color m_whiteAlpha = new Color(1f, 1f, 1f, .2f);
            public bool m_triangleDataUpdated = false;
            public bool m_OldSkinningMethodChecked = false;

            bool m_selectBone = false;
            public bool m_FastDraw = true;
            public int m_selectedCurve = -1;

            Vector2 scroll;
            public override void OnInspectorGUI()
            {
                hideWireframe = false;
                eTab lastTab = m_tab;
                base.OnInspectorGUI();

                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;

                if (g == null)
                    return;
#if UNITY_2018_3_OR_NEWER
                if (PrefabUtility.IsPartOfPrefabAsset(g))
#else
                if (PrefabUtility.GetPrefabParent(g.gameObject) == null && PrefabUtility.GetPrefabObject(g.gameObject) != null)
#endif
                {
                    //GUILayout.Label("Instantiate the prefab for modifications", EditorStyles.helpBox);
                    return;
                }

                if (g.m_hd == null)
                    return;

                GUI.SetNextControlName("TAB");

                GUIContent[] toolbar = new GUIContent[] {
                    //new GUIContent("Info",(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.INFO))),
                    new GUIContent("Design",(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.DESIGN))),
                    new GUIContent("Material",(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.MATERIAL))),
                    new GUIContent("Motion",(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.MOTION)))
                };

                m_tab = (eTab)Toolbar((int)m_tab, toolbar);

                

                if (m_tab != lastTab)
                    GUI.FocusControl("");               

                
                if (g == null)
                    return;

                if (g.m_layerType == HairDesignerBase.eLayerType.NONE)//fix old versions
                    g.m_layerType = HairDesignerBase.eLayerType.LONG_HAIR_POLY;

                g.m_params.m_gravityFactor = 0f;//gravity disabled for shader - use bone physics instead


                if (!g.IsActive)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Layer is disabled.", MessageType.Warning);
                    if (GUILayout.Button("Enable", GUILayout.Height(40)))
                        g.SetActive(true);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }



                /*
                if (m_tab == eTab.INFO)
                {
                    if (m_hairDesignerEditor != null)
                        m_hairDesignerEditor.RestorePose();

                    //base.OnInspectorGUI();
                    //GUILayout.Label("Mesh Generator");
                    //g.m_requirePainting = false;
                    //HairDesignerEditor.m_showBrush = false;
                }
                */


                if (m_tab != eTab.DESIGN && lastTab == eTab.DESIGN && !g.m_meshLocked && !Application.isPlaying)
                {
                    //clear the mesh if design mode was not locked
                    g.m_lowPolyMode = true;
                    g.m_needMeshRebuild = true;
                    g.DestroyMesh();
                    g.GenerateMeshRenderer();

                    g.m_lowPolyMode = false;
                    g.m_needMeshRebuild = true;
                    //g.m_forceMirrorUpdate = true;
                    //g.UpdateMirror(true);
                    g.ClearStrandTmpMesh();
                    //g.UpdateMirror(true);
                    g.DestroyMesh();
                    g.GenerateMeshRenderer();
                    //FixBoneScript(g);
                    //Debug.Log("Quit design tab");
                }


                if (m_tab == eTab.DESIGN) 
                {                    
                    if (Application.isPlaying)
                    {
                       EditorGUILayout.HelpBox("Polygon edition disabled in play mode",MessageType.Info);
                    }
                    else if(g.m_meshLocked)
                    {
                        GUILayout.Space(20);
                        EditorGUILayout.HelpBox( "Layer is locked\nAlways lock a layer when painting is over.", MessageType.Info );
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent("Unlock layer", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.UNLOCKED)))))
                        {
                            g.m_hd.m_meshSaved = false;
                            g.m_meshLocked = false;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        hideWireframe = true;
                        g.m_requirePainting = false;
                        HairDesignerEditor.m_showSceneMenu = false;
                    }
                    else
                    {
                        //Mesh not locked

                        DisableSceneTool();
                        g.m_wasInDesignTab = true;
                        if(lastTab != eTab.DESIGN)
                            g.UpdateStrandMesh();

                        if (m_hairDesignerEditor != null)
                        {
                            m_hairDesignerEditor.CreateColliderAndTpose(g.m_skinningVersion160);
                            g.m_meshGenerationTransformReference = HairDesignerEditor.m_meshCollider.transform;
                        }

                        if (CheckBlendshapesModifications())
                        {
                            m_hairDesignerEditor.RemoveCollider();
                            m_hairDesignerEditor.CreateColliderAndTpose(g.m_skinningVersion160);
                            g.m_needMeshRebuild = true;
                            g.GenerateMeshRenderer();
                            //FixBoneScript(g);
                        }



                        //-------------------------------------
                        //update groups for blendshapes
                        
                        if (HairDesignerEditor.m_meshCollider == null)
                        {
                            m_triangleDataUpdated = false;
                        }                        
                        
                        if (!m_triangleDataUpdated && HairDesignerEditor.m_meshCollider != null)
                        {
                            //Update triangle information for current collider
                            for (int i = 0; i < g.m_groups.Count; ++i)
                            {
                                if (g.m_groups[i].m_triLock != null && g.m_groups[i].m_triLock.m_faceId != -1)
                                {  

                                    g.m_groups[i].m_triLock.Apply(
                                        g.m_hd.transform, 
                                        HairDesignerEditor.m_meshCollider.transform,                                         
                                        HairDesignerEditor.m_meshColliderVertices, 
                                        HairDesignerEditor.m_meshColliderTriangles, 
                                        false);
                                    
                                    g.m_groups[i].RemoveTmpStrandData();
                                    g.m_groups[i].Generate();
                                }                               
                                
                            }
                            m_triangleDataUpdated = true;                           
                            
                            g.GenerateMeshRenderer();
                            //FixBoneScript(g);
                        }
                         
                        //-------------------------------------


                        /*
                        if ( g.m_allowFastMeshDraw )
                            g.DestroyMesh();//force hair mesh as single strand rendering
                            */
                        hideWireframe = true;
                        g.m_requirePainting = true;
                        HairDesignerEditor.m_showSceneMenu = true;


                        //g.m_skinningVersion_160 = EditorGUILayout.Toggle("Debug new version", g.m_skinningVersion_160);


                        if (!g.m_skinningVersion160 || g.m_skinningVersion160_Converted)
                        {

                            GUILayout.Label(new GUIContent("Old version", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                            if (!g.m_skinningVersion160_Converted)
                            {
                                EditorGUILayout.HelpBox("Please duplicate the layer before the conversion.",MessageType.Warning);
                                if (GUILayout.Button("Convert to version 1.6"))
                                {
                                    g.m_skinningVersion160 = true;
                                    g.m_skinningVersion160_Converted = true;
                                    for (int i = 0; i < g.m_groups.Count; ++i)
                                    {
                                        int faceId = -1;

                                        Ray r = new Ray(HairDesignerEditor.m_meshCollider.transform.TransformPoint(g.m_groups[i].m_mCurv.startPosition + g.m_groups[i].m_mCurv.startTangent), HairDesignerEditor.m_meshCollider.transform.TransformDirection(-g.m_groups[i].m_mCurv.startTangent));
                                        
                                        RaycastHit[] hits = Physics.RaycastAll(r, 10);
                                        for (int h = 0; h < hits.Length; ++h)
                                        {
                                            if (hits[h].collider == HairDesignerEditor.m_meshCollider)
                                            {
                                                faceId = hits[h].triangleIndex;
                                                break;
                                            }
                                        }


                                        if (faceId != -1)
                                        {
                                            //init triangle

                                            g.m_groups[i].m_triLock.Lock(
                                                g.m_groups[i].m_mCurv.startPosition,
                                                g.m_groups[i].m_mCurv.startTangent,
                                                g.m_groups[i].m_mCurv.GetUp(0),
                                                HairDesignerEditor.m_meshCollider.transform,
                                                g.m_hd.transform,
                                                faceId, 
                                                HairDesignerEditor.m_meshColliderVertices, 
                                                HairDesignerEditor.m_meshColliderTriangles, false);

                                            g.m_groups[i].m_mCurv.ConvertOffsetAndRotation(g.m_groups[i].m_triLock.m_cdata.localPosition, g.m_groups[i].m_triLock.m_cdata.localRotation, false);
                                        }
                                            
                                    }
                                    
                                }
                                TPoseModeChoose(g.m_hd);
                            }
                            else
                            {
                                if (GUILayout.Button("revert to previous version"))
                                {
                                    g.m_skinningVersion160 = false;
                                    g.m_skinningVersion160_Converted = false;
                                    for (int i = 0; i < g.m_groups.Count; ++i)
                                    {
                                        if (g.m_groups[i].m_triLock.m_faceId != -1)
                                        {
                                            //init triangle
                                            //g.m_groups[i].m_triLock.Apply(HairDesignerEditor.m_meshCollider.transform, HairDesignerEditor.m_meshColliderVertices, HairDesignerEditor.m_meshColliderTriangles);
                                            g.m_groups[i].m_mCurv.ConvertOffsetAndRotation(Vector3.zero, Quaternion.identity, true);
                                            g.m_groups[i].m_triLock = null;

                                        }

                                    }
                                }
                            }


                            

                        }






                        GUILayout.Label(new GUIContent("Layer parameters", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                        //bool updateAll = false;
                        EditorGUI.BeginChangeCheck();
                        g.m_scale = EditorGUILayout.FloatField(new GUIContent("Global scale", "strand scale for this layer"), g.m_scale);
                        g.m_groupCreationLength = EditorGUILayout.FloatField("Initial length", g.m_groupCreationLength);
                        g.m_atlasSizeX = EditorGUILayout.IntSlider("Atlas size", g.m_atlasSizeX, 1, 10);


                        if (EditorGUI.EndChangeCheck())
                            g.m_needMeshRebuild = true;


                        //g.m_allowFastMeshDraw = EditorGUILayout.Toggle("Fast draw", g.m_allowFastMeshDraw);
                        //g.m_enableClothPhysics = EditorGUILayout.Toggle("Cloth physics", g.m_enableClothPhysics);
                        g.m_editorData.HandlesUIOrthoSize = EditorGUILayout.FloatField( "Handles ortho size", g.m_editorData.HandlesUIOrthoSize);
                        g.m_editorData.HandlesUIPerspSize = EditorGUILayout.FloatField("Handles persp size", g.m_editorData.HandlesUIPerspSize);

                        
                        GUILayout.Label(new GUIContent("Long hair groups", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                        //m_showSelectionOnly = EditorGUILayout.Toggle("Show edition only", m_showSelectionOnly);

                        int polyCountSelection = 0;
                        int polyCountTotal = 0;



                        //Draw group options
                        for ( int i=0; i<g.m_groups.Count; ++i )
                        {                         

                            if(g.m_groups[i].m_mCurv == null )
                            {
                                //g.m_groups[i].m_mCurv = new MBZCurv( g.m_groups[i].m_curv );
                                g.m_groups[i].m_mCurv = new MBZCurv();
                                g.m_groups[i].Generate();
                            }
                            
                            int triCount = 0;
                            for( int k=0; k< g.m_groups[i].m_strands.Count; ++k)
                            {
                                if( g.m_groups[i].m_strands[k].mesh != null &&  g.m_editorLayers[g.m_groups[i].m_layer].visible )
                                    triCount += g.m_groups[i].m_strands[k].mesh.triangles.Length / 3;
                            }

                            //if(g.m_groups[i].m_edit || !m_showSelectionOnly)
                            if (g.m_groups[i].m_edit)
                                polyCountSelection += triCount;

                            polyCountTotal += triCount;

                            /*
                            if (m_showSelectionOnly && !g.m_groups[i].m_edit)
                            {
                                if (updateAll)
                                    g.m_groups[i].Generate();
                                continue;
                            }
                            */


                            //GUILayout.BeginHorizontal();
                            /*
                            GUILayout.Label("" + (i + 1) + ". ", GUILayout.Width(20));
                            GUILayout.Label("" + triCount + " tri", EditorStyles.boldLabel );
                            */
                            /*
                            //if (GUILayout.Button("Generate"))
                            //    g.m_groups[i].Generate();
                            if (GUILayout.Button(g.m_groups[i].m_edit?"Save":"Edit", GUILayout.Width(50f)))
                                g.m_groups[i].m_edit = !g.m_groups[i].m_edit;

                            if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.DUPLICATE), GUILayout.Height(20), GUILayout.Width(25)))
                            {
                                if (EditorUtility.DisplayDialog("Duplicate", "Duplicate this hair group?\n"+(i+1)+" -> "+ (g.m_groups.Count+1), "Ok", "Cancel"))
                                    g.m_groups.Add(g.m_groups[i].Copy());
                            }

                            if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.TRASH), GUILayout.Height(20), GUILayout.Width(25)))
                            {
                                if(EditorUtility.DisplayDialog("Delete", "Delete this hair group?", "Ok", "Cancel"))
                                    g.m_groups.RemoveAt(i--);
                            }
                            */


                            //GUILayout.EndHorizontal();

                            /*
                            if (g.m_groups[i].m_edit)
                            {
                                List<HairDesignerGeneratorLongHairBase.HairGroup> lst = new List<HairDesignerGeneratorLongHairBase.HairGroup>();
                                lst.Add(g.m_groups[i]);
                                EditGroupParams(lst);
                                g.m_groups[i].m_parentOffset = g.m_groups[i].m_parent.InverseTransformPoint(g.m_hd.transform.TransformPoint(g.m_groups[i].m_mCurv.startPosition));
                                g.m_groups[i].m_parentRotation = Quaternion.Inverse(g.m_groups[i].m_parent.rotation);
                            }
                            
                            if ( (g.m_groups[i].m_edit && GUI.changed) || updateAll)
                                g.m_groups[i].Generate();
                            */

                        }

                        GUILayout.Label("Polycount selection: " + polyCountSelection);
                        GUILayout.Label("Polycount total: " + polyCountTotal);

                        /*
                        if (GUILayout.Button(new GUIContent("new curve", HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))))
                            g.m_groups.Add(new HairDesignerGeneratorLongHairBase.HairGroup());
                        */


                        //g.m_scale = Mathf.Clamp(g.m_scale, 0, g.m_scale);

                        //g.m_strandSpacing = EditorGUILayout.FloatField("Min spacing", g.m_strandSpacing / g.strandScaleFactor) * g.strandScaleFactor;
                        //g.m_strandSpacing = Mathf.Clamp(g.m_strandSpacing, 0, g.m_strandSpacing);






                        if (g.m_hairStrand != null)
                        {
                            //int triCount = (g.m_hairStrand.triangles.Length / 3) * g.GetStandCount();
                            
                            if (polyCountTotal < 30000)
                                EditorGUILayout.HelpBox("Mesh info : " + g.GetStrandCount() + " strands | " + polyCountTotal + " tri", MessageType.Info);
                            else if (polyCountTotal < 65208)
                                EditorGUILayout.HelpBox("You should reduce poly count!\n Check Strand subdivisions or remove some strand\n Mesh info : " + polyCountTotal + " tri", MessageType.Warning);
                            else
                                EditorGUILayout.HelpBox("Too many polygons!\n Reduce Strand resolution or remove some strand\n Mesh info : " + polyCountTotal + " tri", MessageType.Error);
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent("Clear layer", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.TRASH)))))
                        {
                            if (EditorUtility.DisplayDialog("Clear groups", "Delete all strands in current layer ?", "Ok", "Cancel"))
                            {
                                Undo.RecordObject(g, "clear strands");
                                for( int i=0; i<g.m_groups.Count; ++i )
                                    g.m_groups[i].m_strands.Clear();
                                g.m_groups.Clear();
                            }
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(20);

                        EditorGUILayout.HelpBox("Layer is unlocked\nAlways lock a layer when design is over.", MessageType.Info);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent("Lock layer", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.LOCKED)))))
                        {
                            /*
                            for (int i = 0; i < g.m_groups.Count; ++i)
                                for (int j = 0; j < g.m_groups[i].m_strands.Count; ++j)
                                {
                                    DestroyImmediate(g.m_groups[i].m_strands[j].mesh);
                                    g.m_groups[i].m_strands[j].mesh = null;
                                    g.m_groups[i].RemoveTmpStrandData();
                                }
                                */
                                /*
                            g.m_fastMode = true;
                            g.m_needMeshRebuild = true;
                            g.DestroyMesh();
                            g.GenerateMeshRenderer();
                            */
                            
                            g.m_lowPolyMode = false;
                            g.m_needMeshRebuild = true;
                            g.m_forceMirrorUpdate = true;
                            g.ClearStrandTmpMesh();
                            //g.UpdateMirror(true);                                               
                            g.DestroyMesh();
                            g.GenerateMeshRenderer();
                            ReplaceBaseScripts(g);
                            //EditorUtility.SetDirty(target);
                            g.m_needMeshRebuild = false;
                            g.m_meshLocked = true;
                            m_tab = eTab.MATERIAL;
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        

                        GUILayout.Space(20);

                        if (!g.m_materialInitialized)
                        {
                            DrawMaterialSettings();
                            g.m_materialInitialized = true;                            
                        }


                    }
                    

                }
                else if(!Application.isPlaying)
                {
                    //Not in design tab -> Mesh must be generated
                    if (g.m_meshInstance == null)
                    {
                        g.GenerateMeshRenderer();
                        //FixBoneScript(g);
                        //EditorUtility.SetDirty(target);
                    }
                }



                if (m_tab == eTab.MATERIAL)
                {
                    EnableSceneTool();
                    if (m_hairDesignerEditor != null && !Application.isPlaying)
                    {
                        m_triangleDataUpdated = false;
                        m_hairDesignerEditor.RestorePose();
                        if (HairDesignerEditor.m_meshCollider != null)
                            DestroyImmediate(HairDesignerEditor.m_meshCollider.gameObject);
                    }
                    g.m_requirePainting = false;
                    HairDesignerEditor.m_showSceneMenu = false;
                    hideWireframe = true;
                    DrawMaterialSettings();
                    
                }

                /*
                for (int i = 0; i < g.m_groups.Count; ++i)
                {
                    if (g.m_groups[i].m_parent != null)
                    {
                        if (g.m_groups[i].m_bones.Count > 0 && g.m_groups[i].m_bones[0] != null && g.m_groups[i].m_parent != g.transform)
                        {
                            //g.m_groups[i].m_bones[0].transform.position = g.m_groups[i].m_parent.TransformPoint(g.m_groups[i].m_parentOffset);
                            //g.m_groups[i].m_bones[0].transform.rotation = g.m_groups[i].m_parentRotation * g.m_groups[i].m_parent.rotation;
                        }
                    }
                }
                */

                //HairDesignerEditor.m_showMotionZone = false;

                if (m_tab == eTab.MOTION)
                {
                    EnableSceneTool();
                    if (m_hairDesignerEditor != null)
                        m_hairDesignerEditor.CreateColliderAndTpose(g.m_skinningVersion160);

                    hideWireframe = true;
                    HairDesignerEditor.m_showSceneMenu = false;
                    //HairDesignerEditor.m_showMotionZone = true;
                                        
                    MotionZoneInspectorGUI();
                }

               

                //DrawDefaultInspector();

            }






            /// <summary>
            /// 
            /// </summary>
            /// <param name="ms"></param>
            public void DrawMotionSolverSettingsInspector( MotionSolver.SolverSettings ms )
            {
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Solver", EditorStyles.boldLabel);

                ms.solverSteps = EditorGUILayout.IntField("Solver steps", ms.solverSteps);
                ms.solverSteps = Mathf.Clamp(ms.solverSteps,1, 50);

                ms.timeScale = EditorGUILayout.FloatField("Time scale", ms.timeScale);


                ms.updateOnFixedUpdate = EditorGUILayout.Toggle("Fixed update", ms.updateOnFixedUpdate);
                if(!ms.updateOnFixedUpdate)
                {
                    ms.dtStep = EditorGUILayout.FloatField("Time step", ms.dtStep);
                    ms.dtStep = Mathf.Clamp01(ms.dtStep);
                }

                GUILayout.EndVertical();


                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Motion", EditorStyles.boldLabel);

                GUILayout.Label("Root / Tip", EditorStyles.centeredGreyMiniLabel);
                ms.rigidity = EditorGUILayout.Vector2Field("Rigidity", ms.rigidity);
                ms.motionStability = EditorGUILayout.Vector2Field("Motion stability", ms.motionStability);
                ms.shapeFactor = EditorGUILayout.Vector2Field("Shape factor", ms.shapeFactor);
                ms.RootTipSmoothFactor = EditorGUILayout.Vector2Field("Smooth factor", ms.RootTipSmoothFactor);

                GUILayout.Space(5);
                GUILayout.Label("Global", EditorStyles.centeredGreyMiniLabel);
                //ms.distanceSmooth = EditorGUILayout.FloatField("Distance Smooth", ms.distanceSmooth);
                ms.globalSmooth = EditorGUILayout.Slider("Global smooth", ms.globalSmooth, 0f,1f);
                ms.maxAngle = EditorGUILayout.Slider("Max angle", ms.maxAngle, 0f, 180f);
                ms.elasticity = EditorGUILayout.Slider("Elasticity", ms.elasticity,0f,2f); 
                ms.scale = EditorGUILayout.FloatField("Scale", ms.scale);
                ms.scale = Mathf.Max(ms.scale, .01f);

                GUILayout.EndVertical();

                
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Forces", EditorStyles.boldLabel);

                ms.gravityFactor = EditorGUILayout.FloatField("Gravity factor", ms.gravityFactor);

                ms.windMain = EditorGUILayout.FloatField("Wind main", ms.windMain);
                ms.windTurbulence = EditorGUILayout.FloatField("Wind turbulence", ms.windTurbulence);

                ms.customForce = EditorGUILayout.Vector3Field("Custom force", ms.customForce);

                GUILayout.EndVertical();


                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Constraints", EditorStyles.boldLabel);
                
                EditorGUILayoutListField(g, "m_motionSettings.constraints");

                GUILayout.EndVertical();



                /*
                serializedObject.Update();
                SerializedProperty sp = serializedObject.FindProperty("m_motionSettings");
                EditorGUILayout.PropertyField(sp, new GUIContent("Solver settings"), true);
                //g.m_verletSettings.GPU = true;
                serializedObject.ApplyModifiedProperties();
                */
            }





            /// <summary>
            /// 
            /// </summary>
            /// <param name="ms"></param>
            public void DrawMotionSolverColliderInspector(MotionSolver.SolverSettings ms)
            {
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;

                ms.collisionOffset = EditorGUILayout.FloatField("Collision offset", ms.collisionOffset);
                ms.ComputeCollisions = g.m_enableCollision;
                EditorGUILayoutListField(g, "m_motionSettings.colliders");
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if( GUILayout.Button("Add Colliders from skeleton hierachy") )
                {
                    g.AddHairDesignerCollidersFromBoneHierarchy(ms);
                }
                GUILayout.EndHorizontal();

                

            }




            int[] layerIdx = null;
            string[] layerIdxName = null;
            public bool EditGroupParams(List<HairDesignerGeneratorLongHairBase.HairGroup> hgLst )
            {
                if (hgLst.Count == 0)
                    return false;

                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                HairDesignerGeneratorLongHairBase.HairGroup hg = hgLst[0];
                bool newB;
                float newF;
                int newI;
                Vector3 newV3;
                //bool changed = false;
                //AnimationCurve newAC;
                bool guiChanged = false;

                EditorGUI.BeginChangeCheck();
                scroll = GUILayout.BeginScrollView(scroll);

                if (g.m_editorLayers != null)
                {

                    //------------------------
                    //DEBUG
                    /*
                    GUILayout.Label("Version_160 " + g.m_skinningVersion160);
                    if (hg.m_triLock!=null)
                    {
                        GUILayout.Label("Trilock enable ");
                        GUILayout.Label(""+hg.m_triLock.m_faceId);
                        GUILayout.Label("" + hg.m_triLock.m_position.weights[0] + " " + hg.m_triLock.m_position.weights[1] + " " + hg.m_triLock.m_position.weights[2]);
                        GUILayout.Label("trilock pos " + hg.m_triLock.m_cdata.localPosition);
                        if (GUILayout.Button("Fix"))
                        {
                            hg.m_mCurv.m_offset = hg.m_triLock.m_cdata.localPosition;
                            hg.m_mCurv.m_rotation = hg.m_triLock.m_cdata.localRotation;
                        }

                    }
                    GUILayout.Label("Start pos " + hg.m_mCurv.startPosition);
                    GUILayout.Label("Get pos " + hg.m_mCurv.GetPosition(0));
                    GUILayout.Label("offset " + hg.m_mCurv.m_offset);
                    */          
                    //------------------------

                    if (layerIdx == null || layerIdx.Length != g.m_editorLayers.Count )
                    {
                        layerIdxName = new string[g.m_editorLayers.Count];
                        layerIdx = new int[g.m_editorLayers.Count];
                        for (int i = 0; i < g.m_editorLayers.Count; ++i)
                        {
                            layerIdxName[i] = "" + (i + 1);
                            layerIdx[i] = i;
                        }
                    }
                    newI = EditorGUILayout.IntPopup("Layer", hg.m_layer, layerIdxName, layerIdx);
                    //newI = EditorGUILayout.IntField("Layer", hg.m_layer+1);
                    newI = Mathf.Clamp(newI, 0, g.m_editorLayers.Count-1);
                    if (hg.m_layer != newI)
                    {
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_layer = newI;
                    }
                }


                //hg.m_generationMode = (HairDesignerGeneratorLongHairBase.eMeshGenerator)EditorGUILayout.EnumPopup("Mesh generator", hg.m_generationMode);
                hg.m_generationMode = HairDesignerGeneratorLongHairBase.eMeshGenerator.HAIR_CARDS;

                if (hg.m_generationMode == HairDesignerGeneratorLongHairBase.eMeshGenerator.MESH_COLLECTION)
                {
                    //WIP
                    hg.m_strandMeshCollection = EditorGUILayout.ObjectField("Strand Collection", hg.m_strandMeshCollection, typeof(HairDesignerStrandMeshCollectionBase), false) as HairDesignerStrandMeshCollectionBase;
                    if (hg.m_strandMeshCollection != null)
                    {
                        if (hg.m_strandMeshCollection.m_collection.Count > 0)
                        {
                            HairDesignerStrandMeshCollectionBase.StrandMesh sm = hg.m_strandMeshCollection.m_collection.Find(sd => sd.id == hg.m_strandMeshId);
                            if (sm == null)
                                hg.m_strandMeshId = hg.m_strandMeshCollection.m_collection[0].id;

                            string[] options = hg.m_strandMeshCollection.m_collection.Select(sd => sd.name).ToArray();
                            List<int> ids = hg.m_strandMeshCollection.m_collection.Select(sd => sd.id).ToList<int>();


                            int id = EditorGUILayout.Popup("Mesh",ids.IndexOf(hg.m_strandMeshId), options);
                            hg.m_strandMeshId = ids[id];
                        }                        

                    }




                }
                



                //if (hgLst.Count == 1)//update doesn't work with multi selection
                {
                    /*
                    float[] eval = new float[10];
                    for (int test = 0; test < 10; ++test)
                        eval[test] = hg.m_shape.Evaluate((float)test / 10f);
                    */
                    EditorGUI.BeginChangeCheck();
                    hg.m_shape = EditorGUILayout.CurveField("Shape", hg.m_shape);

                    /*
                    bool ACchanged = false;
                    for( int test =0; test<10; ++test )
                    {
                        if (eval[test] != newAC.Evaluate((float)test / 10f))
                            ACchanged = true;
                    }*/
                    //if(ACchanged)
                    if (EditorGUI.EndChangeCheck())
                    {
                        //Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit shape");
                        for (int i = 1; i < hgLst.Count; ++i)
                        {
                            hgLst[i].m_shape = new AnimationCurve(hg.m_shape.keys);
                        }
                        guiChanged = true;
                    }
                }



                
                
                //hg.m_strandMeshId = EditorGUILayout.IntField(hg.m_strandMeshId);

                newF = EditorGUILayout.FloatField("Scale", hg.m_scale*100f)/100f;
                if (hg.m_scale != newF)
                {
                    guiChanged = true;
                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    for (int i = 0; i < hgLst.Count; ++i)
                        hgLst[i].m_scale = newF;
                }



                

                newF = EditorGUILayout.FloatField("Start angle", hg.m_mCurv.m_startAngle);
                if (hg.m_mCurv.m_startAngle != newF)
                {
                    guiChanged = true;
                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    for (int i = 0; i < hgLst.Count; ++i)
                    {
                        //hgLst[i].m_mCurv.m_startAngle = newF;
                        hgLst[i].m_mCurv.SetStartAngle(0,newF);
                    }
                }

                newF = EditorGUILayout.FloatField("End angle", hg.m_mCurv.m_endAngle);
                if (hg.m_mCurv.m_endAngle != newF)
                {
                    guiChanged = true;
                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    for (int i = 0; i < hgLst.Count; ++i)
                    {
                        //hgLst[i].m_mCurv.m_endAngle = newF;
                        hgLst[i].m_mCurv.SetEndAngle(hgLst[i].m_mCurv.m_curves.Length-1, newF);
                    }
                }


                if (hg.m_generationMode == HairDesignerGeneratorLongHairBase.eMeshGenerator.MESH_COLLECTION)
                {

                    newI = EditorGUILayout.IntField("Bone count", hg.m_boneCount);
                    newI = Mathf.Clamp(newI, 1, 50);
                    if (hg.m_strandCount != newI)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_boneCount = newI;
                    }
                }



                if (hg.m_generationMode == HairDesignerGeneratorLongHairBase.eMeshGenerator.HAIR_CARDS)
                {
                    newI = EditorGUILayout.IntField("strand count", hg.m_strandCount);
                    newI = Mathf.Clamp(newI, 1, 50);
                    if (hg.m_strandCount != newI)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_strandCount = newI;
                    }

                    newF = EditorGUILayout.FloatField("strand max angle", hg.m_strandMaxAngle);
                    if (hg.m_strandMaxAngle != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_strandMaxAngle = newF;
                    }

                    
                    newF = EditorGUILayout.FloatField("Normal offset", hg.m_normalOffset);
                    if (hg.m_normalOffset != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_normalOffset = newF;
                    }

                    newF = EditorGUILayout.FloatField("Tangent start offset", hg.m_tangentStartOffset);
                    if (hg.m_tangentStartOffset != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_tangentStartOffset = newF;
                    }

                    

                    newI = EditorGUILayout.IntField("subdivision X", hg.m_subdivisionX);
                    newI = Mathf.Clamp(newI, 1, 10);
                    if (hg.m_subdivisionX != newI)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_subdivisionX = newI;
                    }

                    newI = EditorGUILayout.IntField("subdivision Y", hg.m_subdivisionY);
                    newI = Mathf.Clamp(newI, 1, 50);
                    if (hg.m_subdivisionY != newI)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_subdivisionY = newI;
                    }

                    newF = EditorGUILayout.FloatField("bend start", hg.m_bendAngleStart);
                    if (hg.m_bendAngleStart != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_bendAngleStart = newF;
                    }

                    newF = EditorGUILayout.FloatField("bend end", hg.m_bendAngleEnd);
                    if (hg.m_bendAngleEnd != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_bendAngleEnd = newF;
                    }

                    newF = EditorGUILayout.FloatField("folding", hg.m_folding);
                    if (hg.m_folding != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_folding = newF;
                    }

                    
                    newF = EditorGUILayout.FloatField("Wave amplitude", hg.m_waveAmplitude);
                    if (hg.m_waveAmplitude != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_waveAmplitude = newF;
                    }

                    newF = EditorGUILayout.FloatField("Wave period", hg.m_wavePeriod);
                    if (hg.m_wavePeriod != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_wavePeriod = newF;
                    }

                    /*
                    newF = EditorGUILayout.FloatField("Start offset", hg.m_startOffset);
                    if (hg.m_startOffset != newF)
                    {
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_startOffset = newF;
                    }

                    newF = EditorGUILayout.FloatField("End offset", hg.m_endOffset);
                    if (hg.m_endOffset != newF)
                    {
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_endOffset = newF;

                    }*/

                    newV3 = EditorGUILayout.Vector3Field("Start offset", hg.m_startOffsetV3);
                    if (hg.m_startOffsetV3 != newV3)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_startOffsetV3 = newV3;
                    }

                    newV3 = EditorGUILayout.Vector3Field("End offset", hg.m_endOffsetV3);
                    if (hg.m_endOffsetV3 != newV3)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_endOffsetV3 = newV3;
                    }


                    newI = EditorGUILayout.IntField("Rnd seed", hg.m_rndSeed);
                    if (hg.m_rndSeed != newI)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_rndSeed = newI;
                    }

                    newF = EditorGUILayout.Slider("Rnd length", hg.m_rndStrandLength, 0f, 1f);
                    if (hg.m_rndStrandLength != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_rndStrandLength = newF;
                    }




                    hg.m_normalSwitch = 0f;
                    /*
                    newF = EditorGUILayout.Slider("Normal switch", hg.m_normalSwitch, 0f, 1f);
                    if (hg.m_normalSwitch != newF)
                    {
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_normalSwitch = newF;
                    }*/

                    newI = EditorGUILayout.IntField("UV X", hg.m_UVX);
                    newI = Mathf.Clamp(newI, 1, 10);
                    if (hg.m_UVX != newI)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_UVX = newI;
                    }

                }




                //hg.m_parent = EditorGUILayout.ObjectField("parent", hg.m_parent, typeof(Transform),true) as Transform;
                Transform selectedBone = SelectBone(hg.m_parent);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (g.m_hd.m_smr != null)
                {
                    if (GUILayout.Button("select bone", EditorStyles.miniButton))
                        m_selectBone = !m_selectBone;
                }
                GUILayout.EndHorizontal();

                if (selectedBone != null)
                {
                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    //hg.m_parent = selectedBone;
                    for (int i = 0; i < hgLst.Count; ++i)
                        hgLst[i].m_parent = selectedBone;
                }



                newF = EditorGUILayout.Slider("Bones start offset", hg.m_bonesStartOffset, 0f, 1f);
                if (hg.m_bonesStartOffset != newF)
                {
                    guiChanged = true;
                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    for (int i = 0; i < hgLst.Count; ++i)
                        hgLst[i].m_bonesStartOffset = newF;
                }
                


                newB = EditorGUILayout.Toggle("Snap surface", hg.m_snapToSurface);
                if (hg.m_snapToSurface != newB)
                {
                    guiChanged = true;
                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    for (int i = 0; i < hgLst.Count; ++i)
                    {
                        hgLst[i].m_snapToSurface = newB;
                        

                        if (g.m_skinningVersion160)
                        {
                            if (newB)
                            {

                                //hgLst[i].m_mCurv.ConvertOffsetAndRotation(hgLst[i].m_triLock.m_cdata.localPosition, hgLst[i].m_triLock.m_cdata.localRotation);                                
                                SnapToSurface(g, g.m_hd.transform, hgLst[i]);
                            }
                            else
                            {
                                hgLst[i].m_triLock.m_faceId = -1;
                                hgLst[i].m_mCurv.ConvertOffsetAndRotation(Vector3.zero, Quaternion.identity, false);
                            }
                        }
                    }
                }

                newB = EditorGUILayout.Toggle("Dynamic", hg.m_dynamic);
                if (hg.m_dynamic != newB)
                {
                    guiChanged = true;
                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                    for (int i = 0; i < hgLst.Count; ++i)
                        hgLst[i].m_dynamic = newB;
                }                
                GUI.enabled = hg.m_dynamic;



                if (g.m_motionData.motionSystem != HairDesignerGeneratorLongHairBase.eMotionSystem.V3)
                {
                    //per strand custom params not yet available for V3

                    newB = EditorGUILayout.Toggle("Collisions", hg.m_enableCollision);
                    if (hg.m_enableCollision != newB)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_enableCollision = newB;
                    }

                    newF = EditorGUILayout.Slider("Gravity", hg.m_gravityFactor, 0f, 1f);
                    if (hg.m_gravityFactor != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_gravityFactor = newF;
                    }


                    newF = EditorGUILayout.Slider("Root rigidity", hg.m_rootRigidity, 0f, 1f);
                    if (hg.m_rootRigidity != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_rootRigidity = newF;
                    }

                    newF = EditorGUILayout.Slider("Rigidity", hg.m_rigidity, 0f, 1f);
                    if (hg.m_rigidity != newF)
                    {
                        guiChanged = true;
                        Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                        for (int i = 0; i < hgLst.Count; ++i)
                            hgLst[i].m_rigidity = newF;
                    }

                }
                //bool changed = EditorGUI.EndChangeCheck();

                GUI.enabled = true;

                /*
                //int splitCount = EditorGUILayout.IntField("Curve split", hg.m_mCurv.m_curves.Length);                               
                int splitCount = 1;
                if (hg.m_mCurv.m_curves.Length != splitCount)
                    hg.m_mCurv.Split(splitCount);
                    */

                GUILayout.EndScrollView();

                if (guiChanged)
                {
                    //Debug.Log("changed");
                    for (int i = 0; i < hgLst.Count; ++i)
                    {
                        for (int j = 0; j < hgLst[i].m_strands.Count; ++j)
                        {
                            if(hgLst[i].m_strands[j].tmpData!=null)
                                hgLst[i].m_strands[j].tmpData.needRefresh = true;
                        }
                        hgLst[i].Generate();
                    }
                    return true;
                }

                return false;
            }



            Matrix4x4[] m_meshMatrix = null;
            void DrawStrandMesh( HairDesignerGeneratorLongHairBase g )
            {
                bool needStrandRefresh = false;
                g.m_generateStrandMeshOnly = true;

                if(m_meshMatrix == null)
                {
                    m_meshMatrix = new Matrix4x4[1];                    
                }

                m_meshMatrix[0] = Matrix4x4.TRS(g.transform.position, g.transform.rotation, g.transform.localScale);

                for (int i = 0; i < g.m_groups.Count; ++i)
                {                    
                    for (int j = 0; j < g.m_groups[i].m_strands.Count; ++j)
                    {
                        if (g.m_editorLayers.Count > g.m_groups[i].m_layer && !g.m_editorLayers[g.m_groups[i].m_layer].visible)
                        {                        
                            continue;
                        }

                        if (g.m_groups[i].m_strands[j].mesh != null)
                        {
                            if (g.m_hairMeshMaterial != null)
                            {
                                if(HairDesignerEditor.GetRenderPipeline() == HairDesignerEditor.eRP.STANDARD)
                                    Graphics.DrawMesh(g.m_groups[i].m_strands[j].mesh, g.transform.position, g.transform.rotation, g.m_hairMeshMaterial, 0, null, 0, g.m_matPropBlkHair, true, true);
                                else
                                    Graphics.DrawMeshInstanced(g.m_groups[i].m_strands[j].mesh, 0, g.m_hairMeshMaterial, m_meshMatrix, 1, g.m_matPropBlkHair);
                            }

                            if (g.m_hairMeshMaterialTransparent != null)
                            {
                                if (HairDesignerEditor.GetRenderPipeline() == HairDesignerEditor.eRP.STANDARD)
                                    Graphics.DrawMesh(g.m_groups[i].m_strands[j].mesh, g.transform.position, g.transform.rotation, g.m_hairMeshMaterialTransparent, 0, null, 0, g.m_matPropBlkHair, true, true);
                                else
                                    Graphics.DrawMeshInstanced(g.m_groups[i].m_strands[j].mesh, 0, g.m_hairMeshMaterialTransparent, m_meshMatrix, 1, g.m_matPropBlkHair);
                            }
                        }
                        else
                        {
                            needStrandRefresh = true;
                        }
                    }
                }

                if (needStrandRefresh)
                {
                    g.GenerateGroupBones();
                    g.UpdateStrandMesh();
                }
            }
                       



            public void DrawToolBar()
            {

            }





            static bool m_showSelectionPanel = true;
            /// <summary>
            /// DrawSceneMenu
            /// </summary>
            /// <param name="width"></param>
            public override void DrawSceneMenu(float width)
            {
                
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;

                if (g == null)
                    return;
                //m_currentTool = ePaintingTool.ADD;
                
                GUILayout.BeginHorizontal();

                /*
                GUI.color = m_currentTool == ePaintingTool.ADD ? Color.white : HairDesignerEditor.unselectionColor;
                if (GUILayout.Button(new GUIContent("Curves", HairDesignerEditor.Icon(HairDesignerEditor.eIcons.PAINT)), GUILayout.MaxWidth(width)))
                {
                    //m_currentTool = m_currentTool== ePaintingTool.NONE ? ePaintingTool.ADD : ePaintingTool.NONE; ;
                    m_currentTool = ePaintingTool.ADD;
                }

                GUI.color = m_currentTool == ePaintingTool.NONE ? Color.white : HairDesignerEditor.unselectionColor;
                if (GUILayout.Button(new GUIContent("Mirror", HairDesignerEditor.Icon(HairDesignerEditor.eIcons.MIRROR)), GUILayout.MaxWidth(width)))
                {
                    //m_currentTool = m_currentTool == ePaintingTool.ADD ? ePaintingTool.NONE : ePaintingTool.ADD;
                    m_currentTool = ePaintingTool.NONE;
                }
                */


                if (m_FastDraw)
                {
                    //g.UpdateMirror(false);
                    DrawStrandMesh(g);
                    if (g.m_skinnedMesh != null)
                        g.m_skinnedMesh.enabled = false;
                }
                else
                {
                    if (g.m_skinnedMesh != null)
                        g.m_skinnedMesh.enabled = true;
                }
                




                GUI.color = m_SceneTab == eTab.CURVE ? Color.white : HairDesignerEditor.unselectionColor;
                if (GUILayout.Button(new GUIContent("Curves", HairDesignerEditor.Icon(HairDesignerEditor.eIcons.CURVE)), GUILayout.MaxWidth(width)))
                {
                    //m_currentTool = m_currentTool== ePaintingTool.NONE ? ePaintingTool.ADD : ePaintingTool.NONE; ;
                    m_SceneTab = eTab.CURVE;
                    m_currentTool = ePaintingTool.ADD;
                }

                GUI.color = m_SceneTab == eTab.MIRROR ? Color.white : HairDesignerEditor.unselectionColor;
                if (GUILayout.Button(new GUIContent("Mirror", HairDesignerEditor.Icon(HairDesignerEditor.eIcons.MIRROR)), GUILayout.MaxWidth(width)))
                {
                    //m_currentTool = m_currentTool == ePaintingTool.ADD ? ePaintingTool.NONE : ePaintingTool.ADD;
                    m_SceneTab = eTab.MIRROR;
                    m_currentTool = ePaintingTool.NONE;
                }


                GUI.color = Color.white;
                GUILayout.EndHorizontal();




                //m_showSelectionButtons = EditorGUILayout.Toggle("Show hair selection", m_showSelectionButtons);
                //m_showSelectionButtons = Event.current.control;

                //if (m_currentTool == ePaintingTool.ADD)
                if (m_SceneTab == eTab.CURVE)
                {
                    //spline tools

                    int old_id = m_subToolId;// (m_currentTool == ePaintingTool.ADD ? 0 : 1);
                    string[] tbTxt = { "Create", "Edit" };
                    m_subToolId = Toolbar(old_id, tbTxt);
                    m_currentTool = ( m_subToolId == 0 ? ePaintingTool.ADD : ePaintingTool.NONE );
                    if(old_id != m_subToolId)
                    {
                        if( m_currentTool == ePaintingTool.ADD )
                        {
                            //disable all the curve selections
                            for(int i=0; i<g.m_groups.Count; ++i)
                            {
                                g.m_groups[i].m_edit = false;
                            }
                        }
                    }



                    if (m_currentTool == ePaintingTool.ADD)
                    {
                        EditorGUILayout.HelpBox(
                            "Ctrl + click  : new curve\n" +
                            "Shift : curve selection\n" +
                            "Shift + Ctrl : delete curve\n" +
                            "Shift + Alt : duplicate curve"
                            , MessageType.Info);

                        m_brushRadius = .01f;
                        m_brushFalloff = 1f;

                    }
                    else
                    {

                        EditorGUILayout.HelpBox(
                            "Ctrl  : rotation handle / split curve\n" +
                            "Shift : curve selection\n" +
                            "Shift + Ctrl : delete hair\n" +
                            "Shift + Alt : duplicate hair\n" +
                            "Alt : move nodes' children\n"
                            , MessageType.Info);


                        
                       

                        m_brushRadius = .0f;
                        m_brushFalloff = 0f;

                        bool selectAll = false;
                        bool unselectAll = false;

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button((m_showSelectionPanel ? "-" : "+") + " Selection", EditorStyles.helpBox, GUILayout.Width(70)))
                            m_showSelectionPanel = !m_showSelectionPanel;
                        //GUILayout.Label("Selected");
                        if (m_showSelectionPanel)
                        {
                            GUILayout.FlexibleSpace();
                            GUI.color = Color.grey;
                            if (GUILayout.Button("all", EditorStyles.helpBox, GUILayout.Width(50)))
                                selectAll = true;
                            if (GUILayout.Button("none", EditorStyles.helpBox, GUILayout.Width(50)))
                                unselectAll = true;
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        int count = 0;

                        if (m_showSelectionPanel)
                        {
                            int groupID = 0;

                            for (int i = 0; i < g.m_groups.Count; ++i)
                            {
                                if (selectAll)
                                    g.m_groups[i].m_edit = true;

                                if (unselectAll)
                                    g.m_groups[i].m_edit = false;

                                if (g.m_groups[i].m_modifierId == -1)
                                    groupID++;
                                else
                                    continue;//don't show hair group generated by modifier

                                if (g.m_groups[i].m_layer != m_currentEditorLayer)
                                {
                                    g.m_groups[i].m_edit = false;
                                    continue;
                                }

                                GUI.color = g.m_groups[i].m_edit ? Color.white : Color.grey;

                                if (GUILayout.Button("" + (groupID), GUILayout.Width(25)))
                                {
                                    g.m_groups[i].m_edit = !g.m_groups[i].m_edit;                                    
                                }

                                count++;
                                if (count % 8 == 0)
                                {
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                }

                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(20);
                        GUI.color = Color.white;
                        List<HairDesignerGeneratorLongHairBase.HairGroup> lst;

                        lst = g.m_groups.Where(group => group.m_edit && group.m_modifierId == -1).ToList();


                        if (EditGroupParams(lst))
                        {
                            if(!m_FastDraw)
                                g.m_needMeshRebuild = true;
                            //g.m_buildSelectionOnly = true;
                        }

                    }
                }

                if(m_SceneTab == eTab.MIRROR)
                //if (m_currentTool == ePaintingTool.NONE)
                {
                    //modifiers menu
                    
                    for( int i=0; i< g.m_mirrorModifiers.Count; ++i)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.BeginHorizontal();
                        g.m_mirrorModifiers[i].name = EditorGUILayout.TextField(g.m_mirrorModifiers[i].name, EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.TRASH), GUILayout.Height(20)))
                        {
                            for( int j=0; j<g.m_groups.Count; ++j )
                            {
                                if (g.m_groups[j].m_modifierId == g.m_mirrorModifiers[i].id)
                                    g.m_groups.RemoveAt(j--);
                            }
                            g.m_mirrorModifiers.Remove(g.m_mirrorModifiers[i--]);
                            if (!m_FastDraw)
                                g.m_needMeshRebuild = true;

                            continue;
                        }                        
                        GUILayout.EndHorizontal();
                        DrawMirrorModifierUI( g.m_mirrorModifiers[i] );
                        GUILayout.EndVertical();                        
                    }

                    

                    if (GUILayout.Button("New mirror"))
                    {
                        HairDesignerGeneratorLongHairBase.MirrorModifier mirror = new HairDesignerGeneratorLongHairBase.MirrorModifier();
                        mirror.id = g.GenerateModifierId();
                        mirror.name = "mirror " + ( g.m_mirrorModifiers.Count+1);
                        g.m_mirrorModifiers.Add(mirror);
                        
                    }
                }

                /*
                //update modifiers
                for (int i = 0; i < g.m_mirrorModifiers.Count; ++i)
                    if (g.m_mirrorModifiers[i].autoUpdate && g.m_mirrorModifiers[i].layers.Contains(m_currentEditorLayer))
                        g.m_mirrorModifiers[i].Update(g);
                        */
            }



            /// <summary>
            /// Draw motion zone tools in scene view
            /// </summary>
            void MotionZoneInspectorGUI()
            {
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;

                GUILayout.Label(new GUIContent("Bones motion ", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                //g.m_gravityFactor = EditorGUILayout.FloatField("Gravity", g.m_gravityFactor);
                //g.m_motionFactor = EditorGUILayout.FloatField("Motion factor", g.m_motionFactor);
                EditorGUI.BeginChangeCheck();


                GUI.enabled = !Application.isPlaying;

                g.m_motionData.motionSystem = (HairDesignerGeneratorLongHairBase.eMotionSystem)EditorGUILayout.EnumPopup("Motion system", g.m_motionData.motionSystem);
                GUI.enabled = true;


                if (g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V1
                    || g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V2
                    )
                {
                    g.m_motionData.gravity = EditorGUILayout.FloatField("Gravity", g.m_motionData.gravity);
                    g.m_motionData.rootRigidity = EditorGUILayout.Slider("Root rigidity", g.m_motionData.rootRigidity, 0f, 1f);
                    g.m_motionData.rigidity = EditorGUILayout.Slider("Rigidity", g.m_motionData.rigidity, 0f, 1f);
                    g.m_motionData.elasticity = EditorGUILayout.Slider("Elasticity", g.m_motionData.elasticity, 0f, 1f);
                    g.m_motionData.smooth = EditorGUILayout.Slider("Smooth", g.m_motionData.smooth, 0f, 1f);
                }

#if HAIRDESIGNER_MOTIONSYSTEM_V3
                if (g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V3)
                {

                    
                    DrawMotionSolverSettingsInspector( g.m_motionSettings );

                   
                }
#endif

                if (g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V2)
                {
                    g.m_motionData.length = EditorGUILayout.Slider("Length", g.m_motionData.length, 0f, 2f);
                    g.m_motionData.parentTransmission = EditorGUILayout.Slider("Parent transmission", g.m_motionData.parentTransmission, 0f, 10f);
                    g.m_motionData.motionFactor = EditorGUILayout.Vector2Field("Motion (Root/Tip)", g.m_motionData.motionFactor);
                    g.m_motionData.centrifugalFactor = EditorGUILayout.Vector2Field("Centrifugal (Root/Tip)", g.m_motionData.centrifugalFactor);
                    g.m_motionData.tipWeight = EditorGUILayout.FloatField("Tip weight", g.m_motionData.tipWeight);
                }


                if (g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V1)
                    g.m_motionData.accurateBoneRotation = EditorGUILayout.Toggle("Accurate bone rotation", g.m_motionData.accurateBoneRotation);


                if (g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V1 ||
                    g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V2)
                {
                    g.m_motionData.bonePID.m_params.kp = EditorGUILayout.FloatField("Damping", g.m_motionData.bonePID.m_params.kp);
                    g.m_motionData.bonePID.m_params.ki = EditorGUILayout.FloatField("Bouncing", g.m_motionData.bonePID.m_params.ki);

                    HairDesignerPID bonepid = new HairDesignerPID(g.m_motionData.bonePID.m_params.kp, g.m_motionData.bonePID.m_params.ki, 0);
                    GUILayout.Box(" ", GUILayout.Height(50f), GUILayout.ExpandWidth(true));
                    HairDesignerEditor.GUIDrawPidResponse(bonepid, GUILayoutUtility.GetLastRect(), 5f);
                    g.m_motionData.windMain = EditorGUILayout.Vector2Field("Wind Main (Root/Tip)", g.m_motionData.windMain);
                    g.m_motionData.windTurbulance = EditorGUILayout.Vector2Field("Wind Turbulance (Root/Tip)", g.m_motionData.windTurbulance);
                }


                if (g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V2)
                {
                    g.m_motionData.collisionFactor = EditorGUILayout.CurveField("Collision factor (Root/Tip)", g.m_motionData.collisionFactor);

                    GUILayout.Space(10);
                    GUILayout.Label(new GUIContent("LOD (Beta)", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);
                    g.m_motionData.lod = EditorGUILayout.IntSlider("LOD", g.m_motionData.lod, 1, 30);
                    g.m_motionData.lod = Mathf.Clamp(g.m_motionData.lod, 1, 30);

                    g.m_motionData.freeze = EditorGUILayout.Toggle("Freeze", g.m_motionData.freeze);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(g);
                }

                GUILayout.Space(10);

                //-------------------
                //colliders

                GUILayout.Label(new GUIContent("Colliders", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                g.m_enableCollision = GUILayout.Toggle(g.m_enableCollision, "Enable collisions");

#if HAIRDESIGNER_MOTIONSYSTEM_V3
                if (g.m_motionData.motionSystem == HairDesignerGeneratorLongHairBase.eMotionSystem.V3)
                {
                    //EditorGUILayoutListField(g, "m_verletSettings.colliders");
                    DrawMotionSolverColliderInspector(g.m_motionSettings);
                }
                else
                { 
#endif

                    EditorGUILayoutListField(g, "m_capsuleColliders");

#if HAIRDESIGNER_MOTIONSYSTEM_V3                    
                }
#endif   
                    /*
                    var serializedObject = new SerializedObject(g);
                    var property = serializedObject.FindProperty("m_capsuleColliders");
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(property, true);
                    serializedObject.ApplyModifiedProperties();
                    */
                    GUILayout.Space(10);

                //----------------------
                //cloth
#if UNITY_2017_1_OR_NEWER
                /*
                GUILayout.Label(new GUIContent("Cloth Physics", (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("\n Generate Cloth Physics \n"))
                {                    
                    GenerateClothPhysics(g);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (!g.m_meshLocked)
                    EditorGUILayout.HelpBox("Layer is not locked.\nthe cloth component will be destroyed when you'll edit the polygons.", MessageType.Warning);
                    */
#endif
                //----------------------



                GUILayout.Label(new GUIContent("Motion zones" , (HairDesignerEditor.Icon(HairDesignerEditor.eIcons.BULLET))), EditorStyles.boldLabel);

                for (int i = 0; i < g.m_motionZones.Count; ++i)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal(  );
                    GUILayout.Label("Motion zones " + (i+1), EditorStyles.boldLabel);
                    if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.TRASH), GUILayout.MaxWidth(25)))
                    {
                        if (EditorUtility.DisplayDialog("Motion zone", "Delete motion zone", "Ok", "Cancel"))
                        {
                            Undo.RecordObject(g, "Delete motion zone");
                            g.m_motionZones.RemoveAt(i--);
                        }
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.Label("Position & hierachy", EditorStyles.boldLabel);
                    g.m_motionZones[i].parent = EditorGUILayout.ObjectField("Parent", g.m_motionZones[i].parent, typeof(Transform), true) as Transform;


                    Transform bone = SelectBone(g.m_motionZones[i].parent);

                    if (bone != null)
                    {
                        g.m_motionZones[i].parent = bone;
                        g.m_motionZones[i].localPosition = Vector3.zero;
                    }

                    g.m_motionZones[i].localPosition = EditorGUILayout.Vector3Field("Offset", g.m_motionZones[i].localPosition);
                    g.m_motionZones[i].radius = EditorGUILayout.FloatField("Radius", g.m_motionZones[i].radius);

                    GUILayout.Label("Motion parameters", EditorStyles.boldLabel);

                    g.m_motionZones[i].pid.m_params.kp = EditorGUILayout.FloatField("Damping", g.m_motionZones[i].pid.m_params.kp);
                    g.m_motionZones[i].pid.m_params.ki = EditorGUILayout.FloatField("Bouncing", g.m_motionZones[i].pid.m_params.ki);
                    g.m_motionZones[i].motionLimit.y = EditorGUILayout.FloatField("Limit", g.m_motionZones[i].motionLimit.y);
                    g.m_motionZones[i].motionLimit.y = Mathf.Clamp(g.m_motionZones[i].motionLimit.y, 0, g.m_motionZones[i].motionLimit.y);
                    g.m_motionZones[i].motionLimit.x = -g.m_motionZones[i].motionLimit.y;

                    g.m_motionZones[i].motionFactor = EditorGUILayout.FloatField("Motion factor", g.m_motionZones[i].motionFactor);

                    g.m_motionZones[i].pid.m_params.kd = 0;
                    GUILayout.Box(" ", GUILayout.Height(50f), GUILayout.ExpandWidth(true));
                    g.m_motionZones[i].pid.m_pidX.m_params = g.m_motionZones[i].pid.m_params;
                    HairDesignerPID pid = new HairDesignerPID(g.m_motionZones[i].pid.m_params.kp, g.m_motionZones[i].pid.m_params.ki, 0);
                    //pid.m_params = g.m_motionZones[i].pid.m_params;
                    //pid.m_params.limits = new Vector2(-10, 10);
                    HairDesignerEditor.GUIDrawPidResponse(pid, GUILayoutUtility.GetLastRect(), 5f);


                    GUILayout.EndVertical();


                }

                if (GUILayout.Button("+ New motion Zone +", GUILayout.Height(40)))
                {
                    Undo.RecordObject(g, "New motion zone");
                    MotionZone mz = new MotionZone();
                    mz.parent = g.m_hd.transform;
                    g.m_motionZones.Add(mz);
                }

                EditorGUILayout.HelpBox("The motion zone must encapsulate the hair strands to enable dynamic animation.\nLock the motion zone to the nearest bone for skeleton animation.", MessageType.Info);
                

            }






            public void DrawShortcutsPanel()
            {

                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;

                if (g.m_meshLocked)
                    return;

                float width = 250;                
                float Xpos = 250;
                float Ypos = 0;


                Handles.BeginGUI();
                GUILayout.BeginArea(new Rect(Xpos, Ypos, width, SceneView.currentDrawingSceneView.camera.pixelHeight));
                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MaxWidth(width));

                //GUILayout.Label("Shortcuts", EditorStyles.centeredGreyMiniLabel);

                GUILayout.BeginHorizontal( EditorStyles.helpBox );
                GUILayout.FlexibleSpace();
                GUI.color = m_FastDraw ? Color.white : Color.grey;
                if (GUILayout.Button("Fast Draw", EditorStyles.miniButton))
                {
                    m_FastDraw = !m_FastDraw;
                    g.ClearStrandTmpMesh();
                    g.m_needMeshRebuild = true;                    
                    g.UpdateStrandMesh();
                }

                GUI.color = g.m_lowPolyMode ? Color.white : Color.grey;
                if (GUILayout.Button("Low poly", EditorStyles.miniButton))
                {
                    g.m_lowPolyMode = !g.m_lowPolyMode;
                    g.ClearStrandTmpMesh();
                    if (!m_FastDraw)
                        g.m_needMeshRebuild = true;
                }

                GUI.color = Color.white;
                
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Update Mirror", EditorStyles.miniButton))
                {                    
                    g.ClearStrandTmpMesh();
                    for (int i = 0; i < g.m_mirrorModifiers.Count; ++i)
                        g.m_mirrorModifiers[i].Update(g);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.EndArea();

                Handles.EndGUI();

                
            }




            /// <summary>
            /// Draw tools
            /// </summary>
            public override void DrawSceneTools()
            {
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                if (!g.IsActive)
                    return;

                if (g.m_hd == null)
                    return;

                if (m_tab == eTab.DESIGN)
                {
                    //if ( m_currentTool == ePaintingTool.ADD )
                    if (m_SceneTab == eTab.CURVE)
                        DrawCurveSceneGUI();
                    //if (m_currentTool == ePaintingTool.NONE)
                    if (m_SceneTab == eTab.MIRROR)
                        DrawModifierSceneTool();

                    DrawLayerPanel();

                    DrawShortcutsPanel();
                }

                if (m_tab == eTab.MOTION)
                    MotionZoneSceneGUI();

            }




            //Vector3 rLocalPoint0;
            bool DrawRotateHandle(HairDesignerGeneratorLongHairBase g, HairDesignerGeneratorLongHairBase.HairGroup hg, int i, Transform trans, float handleSize, bool start)
            {
                
                if (Event.current.alt || !Event.current.control || m_currentTool== ePaintingTool.ADD)
                    return false;
                
                bool modified = false;

                float heightFactorHandle = 4f;

                //compatibility with old version
                if (i == 0)
                    hg.m_mCurv.m_curves[i].m_startAngle = hg.m_mCurv.m_startAngle;
                if (i == hg.m_mCurv.m_curves.Length-1)
                    hg.m_mCurv.m_curves[i].m_endAngle = hg.m_mCurv.m_endAngle;

                Vector3 pos = start ? hg.m_mCurv.GetStartPosition(i) : hg.m_mCurv.GetEndPosition(i);
                Vector3 tan = start ? hg.m_mCurv.GetStartTangent(i) : -hg.m_mCurv.GetEndTangent(i);

                
                pos = trans.TransformPoint(pos);
                tan = trans.TransformDirection(tan);

                tan.Normalize();

                Vector3 up = hg.m_mCurv.GetUp(i, start?0f:1f);
                up = trans.TransformDirection(up).normalized;

                //if( Event.current.type == EventType.MouseDown )
                Vector3 rLocalPoint0 = pos + up * (handleSize * heightFactorHandle - handleSize*.5f) ;// / g.m_hd.globalScale;

                //Vector3 rLocalPoint1 =  FreeMoveHandleTransformPoint(trans, rLocalPoint0, Quaternion.LookRotation(-up, tan), handleSize * 2f, Vector3.zero, ConeCap);
                var fmh_1923_78_638749745248048208 = Quaternion.LookRotation(-up, tan); Vector3 rLocalPoint1 = Handles.FreeMoveHandle( rLocalPoint0, handleSize * 2f, Vector3.zero, ConeCap);


                //Vector3 rWorldPoint = trans.TransformPoint(rLocalPoint1);
                Vector3 rWorldPoint = rLocalPoint1;

                //if (Vector3.Distance(rWorldPoint, trans.TransformPoint(rLocalPoint0)) > .0f)
                float dist = Vector3.Distance(rLocalPoint0, rLocalPoint1);


               

                if (dist > .0001f)
                {
                    

                    modified = true;

                    //project point
                    rLocalPoint1 -= Vector3.Dot((rLocalPoint1 - pos), tan) * tan;
                    rLocalPoint0 -= Vector3.Dot((rLocalPoint0 - pos), tan) * tan;

                    float a = Vector3.Angle(rLocalPoint1 - pos, rLocalPoint0 - pos);

                    float angle = start ? hg.m_mCurv.m_curves[i].m_startAngle : hg.m_mCurv.m_curves[i].m_endAngle;

                  a *= Mathf.Sign(
                        Vector3.Dot(tan.normalized,
                        Vector3.Cross(
                            rLocalPoint0 - pos,
                            rLocalPoint1 - pos
                            ).normalized));
                    
                    angle += a;

                    
                    if (start)
                        hg.m_mCurv.m_curves[i].m_startAngle = angle;
                    else
                        hg.m_mCurv.m_curves[i].m_endAngle = angle;
                    
                }

                if (i > 0)
                    hg.m_mCurv.m_curves[i - 1].m_endAngle = hg.m_mCurv.m_curves[i].m_startAngle;
                
                if (i == 0)
                    hg.m_mCurv.m_startAngle = hg.m_mCurv.m_curves[i].m_startAngle;//compatibility with old version

                if (!start && i == hg.m_mCurv.m_curves.Length - 1)
                    hg.m_mCurv.m_endAngle = hg.m_mCurv.m_curves[i].m_endAngle;

                /*
                Handles.DrawWireArc(trans.TransformPoint(pos), trans.TransformDirection(tan), trans.TransformDirection(up), 60f, handleSize * heightFactorHandle);
                Handles.DrawWireArc(trans.TransformPoint(pos), trans.TransformDirection(tan), trans.TransformDirection(up), -60f, handleSize * heightFactorHandle);
                Handles.DrawDottedLine(trans.TransformPoint(pos), rWorldPoint, 10f);
                */

                Handles.DrawWireArc((pos), (tan), (up), 60f, handleSize * heightFactorHandle);
                Handles.DrawWireArc((pos), (tan), (up), -60f, handleSize * heightFactorHandle);
                Handles.DrawDottedLine((pos), rWorldPoint, 10f);


                /*
                if (modified)
                {
                    for (int j = 0; j < hg.m_mCurv.m_curves.Length; ++i)
                    {
                        Debug.Log("" + j + "->  start: " + hg.m_mCurv.m_curves[j].m_startAngle + "   end: " + hg.m_mCurv.m_curves[j].m_endAngle);
                    }
                }*/


                return modified;
            }




            //Color m_bgColor = new Color(1f,1f,1f,.5f);
            bool DrawCurveHandle(HairDesignerGeneratorLongHairBase g, HairDesignerGeneratorLongHairBase.HairGroup hg, int i, Transform trans, float handleSize )
            {
                bool modified = false;

                bool ctrl = Event.current.control;
                    

                //curve.startPosition = FreeMoveHandleTransformPoint(trans, curve.startPosition, Quaternion.identity, handleSize, Vector3.zero, CircleCap);
                float f = (float)i / (float)hg.m_mCurv.m_curves.Length;

                Vector3 oldPos = hg.m_mCurv.GetPosition(i, 0);

                //Handles.color = m_bgColor;

                


                Color hCol = Color.Lerp(  Color.green, Color.red, f );
                hCol.a = ctrl?.1f : .5f;
                Handles.color = hCol;
                Handles.DrawSolidDisc(trans.TransformPoint(oldPos), Camera.current.transform.forward, handleSize * 1.1f);

                hCol.a = .5f;
                Handles.color = hCol;

                if (DrawRotateHandle(g, hg, i, trans, handleSize, true))
                    modified = true;

                
                if (i == hg.m_mCurv.m_curves.Length - 1 && ctrl)
                {
                    hCol = Color.red;
                    hCol.a = .5f;
                    Handles.color = hCol;

                    if (DrawRotateHandle(g, hg, i, trans, handleSize, false))
                        modified = true;
                }

                if (ctrl)
                    return modified;

                //return false;

                Vector3 newPos = FreeMoveHandleTransformPoint(trans, oldPos, Quaternion.identity, handleSize, Vector3.zero, CircleCap);
                hg.m_mCurv.SetStartPosition(i, newPos);

                if (Vector3.Distance(oldPos, newPos) > 0.0001f)
                    modified = true;

                if (Vector3.Distance(oldPos, hg.m_mCurv.GetStartPosition(0)) > 0.0001f && hg.m_snapToSurface && i==0)
                {
                    
                    if (!SnapToSurface(g, trans, hg))
                    {
                        hg.m_mCurv.SetStartPosition(i, oldPos);
                        newPos = oldPos;
                    }
                    else
                    {
                        newPos = hg.m_mCurv.GetStartPosition(i);
                    }
                }

                Vector3 pos = newPos + hg.m_mCurv.GetStartTangent(i);
                Handles.DrawSolidDisc(trans.TransformPoint(pos), Camera.current.transform.forward, handleSize *.5f);
                Vector3 newTan = FreeMoveHandleTransformPoint(trans, pos, Quaternion.identity, handleSize * .5f, Vector3.zero, RectangleCap) - newPos;
                hg.m_mCurv.SetStartTangent(i, newTan);

                if( i>0 )
                    hg.m_mCurv.SetEndTangent(i-1, -newTan);

                Handles.DrawLine(trans.TransformPoint(newPos), trans.TransformPoint(newPos + hg.m_mCurv.GetStartTangent(i)));

                //move other nodes
                if (Event.current.alt)
                {
                    Vector3 delta = newPos - oldPos;

                    if (delta.sqrMagnitude > 0)
                    {
                        //int c = 0;
                        for (int c = i; c < hg.m_mCurv.m_curves.Length; ++c)
                        {
                            
                            Vector3 oldStartPos = hg.m_mCurv.GetStartPosition(c);                            
                            if (c > i)                                                            
                                hg.m_mCurv.SetStartPosition(c, delta + hg.m_mCurv.GetStartPosition(c));
                            

                            if (c == hg.m_mCurv.m_curves.Length - 1)
                                hg.m_mCurv.SetEndPosition(c, delta + hg.m_mCurv.GetEndPosition(c));


                            
                        }
                    }
                }



                //last node
                if (i == hg.m_mCurv.m_curves.Length - 1)
                {
                    hCol = Color.red;
                    hCol.a = .5f;
                    Handles.color = hCol;
                    /*
                    if (DrawRotateHandle(g, hg, i, trans, handleSize, false))
                        modified = true;
                        */
                    oldPos = hg.m_mCurv.GetEndPosition(i);
                    Handles.DrawSolidDisc(trans.TransformPoint(oldPos), Camera.current.transform.forward, handleSize * 1.1f);
                    newPos = FreeMoveHandleTransformPoint(trans, hg.m_mCurv.GetEndPosition(i), Quaternion.identity, handleSize, Vector3.zero, CircleCap);
                    hg.m_mCurv.SetEndPosition(i, newPos);

                    if (Vector3.Distance(oldPos, newPos) > 0.0001f)
                        modified = true;



                    pos = hg.m_mCurv.GetEndPosition(i) - hg.m_mCurv.GetEndTangent(i);
                    Handles.DrawSolidDisc(trans.TransformPoint(pos), Camera.current.transform.forward, handleSize * .5f);
                    newTan = FreeMoveHandleTransformPoint(trans, pos, Quaternion.identity, handleSize / 2f, Vector3.zero, RectangleCap) - hg.m_mCurv.GetEndPosition(i);
                    hg.m_mCurv.SetEndTangent(i, -newTan);

                    Handles.DrawLine(trans.TransformPoint(hg.m_mCurv.GetEndPosition(i)), trans.TransformPoint(-hg.m_mCurv.GetEndTangent(i) + hg.m_mCurv.GetEndPosition(i)));


                }



                return modified;
            }














            /// <summary>
            /// Draw curve in scene
            /// </summary>
            public void DrawCurveSceneGUI()
            {               
                    



                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                if (g.m_groups.Count == 0 || g.m_meshLocked )
                    return;

                if (g.m_hd == null)
                    return;


                bool needRefresh = false;
                //bool IsDraging = false;

                //if (Event.current.type == EventType.MouseDrag)
                //    IsDraging = true;

                /*
                if (Event.current.type == EventType.MouseUp && g.m_buildSelectionOnly)
                {
                    //g.m_buildSelectionOnly = false;
                    needRefresh = true;
                    //Debug.Log("Disable build only");
                }
                */

                    for (int i = 0; i < g.m_groups.Count; ++i)
                {
                    if (g.m_groups[i] == null)
                    {
                        g.m_groups.RemoveAt(i--);
                        continue;
                    }

                    if (g.m_groups[i].m_layer != m_currentEditorLayer)
                        continue;

                    if (g.m_groups[i].m_modifierId != -1)
                        continue;

                    HairDesignerGeneratorLongHairBase.HairGroup hg = g.m_groups[i];
                    //Transform trans = HairDesignerEditor.m_meshCollider.transform; //g.m_hd.transform;
                    Transform trans = g.m_hd.transform;
                    //if (g.m_skinningVersion160)
                    //    trans = HairDesignerEditor.m_meshCollider.transform;


                    /*
                    if (g.m_skinningVersion160 && hg.m_snapToSurface)
                    {

                        Debug.DrawLine(trans.TransformPoint(hg.m_triLock.m_cdata.vPos[0]), trans.TransformPoint(hg.m_triLock.m_cdata.vPos[1]));
                        Debug.DrawLine(trans.TransformPoint(hg.m_triLock.m_cdata.vPos[1]), trans.TransformPoint(hg.m_triLock.m_cdata.vPos[2]));
                        Debug.DrawLine(trans.TransformPoint(hg.m_triLock.m_cdata.vPos[2]), trans.TransformPoint(hg.m_triLock.m_cdata.vPos[0]));

                    }
                    */

                    //---------------------------
                    //Apply morph
                    /*
                    if (g.m_skinningVersion160)
                    {
                        if (hg.m_triLock!= null && hg.m_triLock.m_faceId != -1)//check if the conversion has been done
                        {
                            if (HairDesignerEditor.m_meshCollider != null)
                            {
                                hg.m_triLock.Apply(HairDesignerEditor.m_meshCollider.transform, HairDesignerEditor.m_meshColliderVertices, HairDesignerEditor.m_meshColliderTriangles, false);
                                if (g.m_groups[i].m_snapToSurface)
                                {                                    
                                    hg.m_mCurv.m_offset = hg.m_triLock.m_cdata.localPosition;
                                    hg.m_mCurv.m_rotation = hg.m_triLock.m_cdata.localRotation;
                                }
                            }
                        }
                    }
                    */
                    //---------------------------




                    int selectionMode = -1;
                    m_showSelectionButtons = Event.current.shift;
                    bool ctrl = Event.current.control;
                    bool alt = Event.current.alt;

                    if (hg.m_mCurv != null && hg.m_mCurv.m_curves.Length>0 && m_showSelectionButtons )
                    {
                        Handles.color = hg.m_edit ? Color.white : Color.gray;
                        GUI.color = hg.m_edit ? Color.white : Color.gray;

                        float dot = Vector3.Dot(trans.TransformDirection(hg.m_mCurv.startTangent).normalized, SceneView.currentDrawingSceneView.camera.transform.forward );
                        if (dot < 0f || hg.m_edit )
                        {
                            if (!ctrl && !alt)
                            {
                                for (int j = 0; j < 3; ++j)
                                {
                                    if (Handles.Button(trans.TransformPoint(hg.m_mCurv.GetPosition((float)j/4f)), Quaternion.identity, .001f, .001f, DotCap))
                                    {
                                        selectionMode = 1;
                                    }
                                }
                            }

                            Handles.BeginGUI();
                            Vector3 UIpos = trans.TransformPoint(hg.m_mCurv.endPosition);
                            UIpos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(UIpos);
                            GUILayout.BeginArea(new Rect(UIpos.x, SceneView.currentDrawingSceneView.camera.pixelHeight - UIpos.y, 30f, 30f));
                           
                            
                            if (!Event.current.control && !Event.current.alt )
                            {
                                if (GUILayout.Button("" + (i + 1)))
                                {
                                    selectionMode = 1;
                                }
                            }
                            else if(Event.current.control)
                            {
                                if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.TRASH)))
                                {
                                    selectionMode = 2;
                                }
                            }
                            else if (Event.current.alt)
                            {
                                if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.DUPLICATE)))
                                {
                                    selectionMode = 3;
                                }
                            }
                            
                            
                            GUILayout.EndArea();
                            Handles.EndGUI();
                        }
                    }
                    GUI.color = Color.white;
                    Handles.color = Color.white;

                    if ( selectionMode != -1)
                    {
                        if (selectionMode == 1)
                        {
                            //if (GUILayout.Button("" + (i + 1)))
                            {
                                Undo.RecordObject(g, "Edit hair");
                                hg.m_edit = !hg.m_edit;

                                if (hg.m_edit)
                                {
                                    g.m_referenceId = i;
                                    m_currentTool = ePaintingTool.NONE;//switch to edit mode
                                }
                            }
                        }
                        else if (selectionMode == 2)
                        {
                            //if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.TRASH)))
                            {
                                Undo.RecordObject(g, "Delete hair");
                                g.m_groups.RemoveAt(i--);
                                if (!m_FastDraw)
                                    g.m_needMeshRebuild = true;
                            }
                        }
                        else if (selectionMode == 3)
                        {
                            //if (GUILayout.Button(HairDesignerEditor.Icon(HairDesignerEditor.eIcons.DUPLICATE)))
                            {
                                Undo.RecordObject(g, "Duplicate hair");
                                g.m_groups.Add(g.m_groups[i].Copy());
                                g.m_groups[i].m_edit = false;
                                if (m_changeSeedOnDuplicate)
                                    g.m_groups[g.m_groups.Count - 1].m_rndSeed = Random.Range(0, int.MaxValue);

                            }
                        }
                    }

                    
                    if (m_selectBone)
                    {
                        if (g.m_hd.m_smr != null && g.m_hd.m_smr.bones.Length > 0)
                        {
                            for (int b = 0; b < g.m_hd.m_smr.bones.Length; ++b)
                            {
                                Handles.BeginGUI();
                                Vector3 UIpos = g.m_hd.m_smr.bones[b].position;
                                UIpos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(UIpos);
                                GUILayout.BeginArea(new Rect(UIpos.x, SceneView.currentDrawingSceneView.camera.pixelHeight - UIpos.y, 10f, 10f));
                                if(GUILayout.Button(""))
                                {
                                    Undo.RecordObject(target as HairDesignerGeneratorLongHairBase, "Edit hair");
                                    List<HairDesignerGeneratorLongHairBase.HairGroup> lst = g.m_groups.Where(group => group.m_edit && group.m_modifierId == -1).ToList();
                                    for(int k=0; k< lst.Count; ++k )
                                        lst[k].m_parent = g.m_hd.m_smr.bones[b];
                                    m_selectBone = false;
                                }
                                GUILayout.EndArea();
                                Handles.EndGUI();
                            }
                        }
                    }


                    if (hg.m_parent == null)
                        hg.m_parent = g.m_hd.transform;

                    float handleSize = HandleUtility.GetHandleSize(SceneView.currentDrawingSceneView.camera.transform.position + SceneView.currentDrawingSceneView.camera.transform.forward) * .2f ;
                    if (!SceneView.currentDrawingSceneView.camera.orthographic)
                        handleSize *= g.m_hd.globalScale * .3f * g.m_editorData.HandlesUIPerspSize;
                    else
                        handleSize *= g.m_editorData.HandlesUIOrthoSize;

                    if (m_showSelectionButtons)
                    {
                        float dot = Vector3.Dot(trans.TransformDirection(hg.m_mCurv.startTangent).normalized, SceneView.currentDrawingSceneView.camera.transform.forward);
                        if (dot < 0f || hg.m_edit)
                        {
                            float max = 30f;
                            for (float a = 1; a < max; ++a)
                            {
                                Handles.color = hg.m_edit ? Color.white : Color.grey;
                                Vector3 p1 = trans.TransformPoint(hg.m_mCurv.GetPosition((a - 1) / max));
                                Vector3 p2 = trans.TransformPoint(hg.m_mCurv.GetPosition(a / max));
                                Handles.DrawLine(p1, p2);
                            }
                        }
                    }


                    if (!hg.m_edit || m_showSelectionButtons )                                                               
                        continue;
                    
                    

                    bool changed = false;

                    



                    //draw bone link
                    if (g.m_hd.m_smr != null)
                    {
                        Handles.color = m_whiteAlpha;
                        Handles.DrawDottedLine(trans.TransformPoint(hg.m_mCurv.GetStartPosition(0)), hg.m_parent.position, handleSize * 100f);                        
                    }







                    /*
                    for (int n = 0; n < hg.m_mCurv.m_curves.Length; ++n)
                    {
                        Handles.color = Color.Lerp(Color.green, Color.red, (float)n / (float)hg.m_mCurv.m_curves.Length);
                    }
                    */

                    //return;

                    //---------------------------------------------------------------------------------------
                    //Curves handles               

                    //bool drawNewHandleVersion = true;
                    bool drawNormals = Event.current.control && !Event.current.alt;
                    
                    //if (drawNewHandleVersion )
                    //if(false)
                    {

                        //draw curve
                        int maxPlot = 5 * hg.m_mCurv.m_curves.Length;

                        hg.m_updatePositionPlotEditorPoints = false;
                        //if (!m_plotCurves.ContainsKey(hg.m_mCurv))
                        if (hg.m_tmpPositionPlotEditor == null || hg.m_tmpPositionPlotEditor.Length != maxPlot + 1)
                        {
                            //m_plotCurves.Add(hg.m_mCurv, new Vector3[maxPlot+1]);
                            hg.m_tmpPositionPlotEditor = new Vector3[maxPlot + 1];
                            hg.m_tmpNormalPlotEditor = new Vector3[maxPlot + 1];
                            hg.m_updatePositionPlotEditorPoints = true;
                            //Debug.Log("updatePoints");
                        }
                        hg.m_updatePositionPlotEditorPoints = true;


                        for (int n = 0; n < maxPlot; ++n)
                        {
                            float t0 = (float)n / ((float)maxPlot);
                            float t1 = (float)(n + 1) / ((float)maxPlot);
                            Handles.color = Color.Lerp(Color.green, Color.red, t0);

                            if(hg.m_updatePositionPlotEditorPoints)
                            {
                                hg.m_tmpPositionPlotEditor[n] = hg.m_mCurv.GetPosition(t0);
                                hg.m_tmpPositionPlotEditor[n+1] = hg.m_mCurv.GetPosition(t1);
                                if (drawNormals)
                                    hg.m_tmpNormalPlotEditor[n] = hg.m_mCurv.GetUp(t0) * .1f;                                
                            }


                            Handles.DrawLine(trans.TransformPoint(hg.m_tmpPositionPlotEditor[n]), trans.TransformPoint(hg.m_tmpPositionPlotEditor[n+1]));
                            if(drawNormals)
                            {
                                Vector3 p0 = trans.TransformPoint(hg.m_tmpPositionPlotEditor[n]);
                                Vector3 p1 = trans.TransformPoint(hg.m_tmpPositionPlotEditor[n] + hg.m_tmpNormalPlotEditor[n]);
                                Handles.DrawLine(p0, p0 + (p1-p0).normalized * handleSize);
                            }
                                

                            /*
                            Handles.color = Color.blue;
                            Handles.DrawLine(trans.TransformPoint(hg.m_tmpPositionPlotEditor[n]), trans.TransformPoint(hg.m_tmpPositionPlotEditor[n] + Vector3.Cross( hg.m_tmpNormalPlotEditor[n], hg.m_mCurv.GetTangent(t0).normalized)));

                            Handles.color = Color.magenta;
                            Handles.DrawLine(trans.TransformPoint(hg.m_tmpPositionPlotEditor[n]), trans.TransformPoint(hg.m_tmpPositionPlotEditor[n] + hg.m_mCurv.GetTangent(t0) * .1f));
                            */

                            //Handles.DrawLine(trans.TransformPoint(hg.m_mCurv.GetPosition(t0)), trans.TransformPoint(hg.m_mCurv.GetPosition(t1)));
                            //Handles.DrawLine(trans.TransformPoint(hg.m_mCurv.GetPosition(t0)), trans.TransformPoint(hg.m_mCurv.GetPosition(t0) + hg.m_mCurv.GetUp(t0)*.1f));
                        }
                        hg.m_updatePositionPlotEditorPoints = false;

                        if ( hg.m_bonesStartOffset > 0 )
                        {
                            Handles.color = Color.yellow;
                            Handles.DrawWireDisc(trans.TransformPoint(hg.m_mCurv.GetPosition(hg.m_bonesStartOffset)), trans.TransformDirection(hg.m_mCurv.GetTangent(hg.m_bonesStartOffset)), handleSize * .2f);
                        }


                        //Draw split/merge
                        if ( /*Event.current.alt &&*/ Event.current.control)
                        {
                            for (int c = 0; c < hg.m_mCurv.m_curves.Length; ++c)
                            {
                                {
                                    Vector3 middle = trans.TransformPoint(hg.m_mCurv.GetPosition(c, .5f));
                                    float s = handleSize * .3f;
                                    Handles.color = Color.blue;
                                    if (Handles.Button(middle, Camera.current.transform.rotation, s, s, Handles.DotHandleCap))
                                    {
                                        hg.m_mCurv.Split(c);
                                        changed = true;
                                    }
                                }

                                if (c > 0 && c < hg.m_mCurv.m_curves.Length)
                                {
                                    Vector3 pos = trans.TransformPoint(hg.m_mCurv.GetPosition(c, 0f));
                                    float s = handleSize * .2f;
                                    Handles.color = Color.red;
                                    if (Handles.Button(pos, Camera.current.transform.rotation, s, s, Handles.DotHandleCap))
                                    {
                                        hg.m_mCurv.RemoveControlPoint(c);
                                        changed = true;
                                    }
                                }

                            }
                        }

                                                
                        //DrawCurveHandle(hg.m_mCurv, 0, trans, handleSize)
                        
                        if (!(Event.current.alt && Event.current.control))
                        {
                            for (int c = 0; c < hg.m_mCurv.m_curves.Length; ++c)
                            {

                                if (DrawCurveHandle(g, hg, c, trans, handleSize))
                                {
                                    changed = true;
                                }
                            }
                        }
                        

                    }





                    


                    if (GUI.changed)
                        changed = true;

                    


                    if ((hg.m_edit && changed) || needRefresh)
                    {
                        //Debug.Log("Rebuild called");
                        //if (IsDraging)
                        //    g.m_buildSelectionOnly = true;
                        hg.RemoveTmpStrandData();
                        hg.Generate();

                        //if (!g.m_allowFastMeshDraw)
                        if (!m_FastDraw)
                        {   
                            //hg.m_tmpPositionPlotEditor = null;
                            hg.m_updatePositionPlotEditorPoints = true;
                            g.m_needMeshRebuild = true;
                        }
                        else
                        {
                            g.UpdateStrandMesh();
                        }
                    }


                   

                }
            }



            /// <summary>
            /// PaintToolAction
            /// </summary>
            /// <param name="svCam"></param>
            public override void PaintToolAction()
            {

                //HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;

                Camera svCam = SceneView.currentDrawingSceneView.camera;
                if (Event.current.alt || HairDesignerEditor.m_meshCollider == null)
                    return;

                /*
                if (m_currentTool != ePaintingTool.ADD)
                    return;
                */ 

                //Debug.Log("PaintToolAction");

                m_CtrlMode = Event.current.control;
                m_ShiftMode = Event.current.shift;
                m_AltMode = Event.current.alt;

                if ( !m_CtrlMode || m_ShiftMode )
                    return;

                HairDesignerBase hd = (target as HairDesignerGenerator).m_hd;

                Vector3 mp = Event.current.mousePosition* EditorGUIUtility.pixelsPerPoint;
                mp.y = svCam.pixelHeight - mp.y;

                /*
                if (m_currentTool == ePaintingTool.ADD)
                {
                    float pixelSize = Vector3.Distance(svCam.WorldToScreenPoint(svCam.transform.position + svCam.transform.forward), svCam.WorldToScreenPoint(svCam.transform.position + svCam.transform.forward + svCam.transform.right * m_brushSize));
                    Vector2 rnd = Random.insideUnitCircle * pixelSize;
                    mp.x += rnd.x;
                    mp.y += rnd.y;
                }*/

                Ray r = svCam.ScreenPointToRay(mp);

                //Setup painting info
                StrandData dt = new StrandData();
                dt.layer = m_currentEditorLayer;
                Vector2 mp2d = Event.current.mousePosition* EditorGUIUtility.pixelsPerPoint;
                mp2d.y = svCam.pixelHeight - mp2d.y;

                BrushToolData bt = new BrushToolData();
                bt.mousePos = mp2d;
                bt.transform = hd.transform;
                bt.tool = m_currentTool;
                bt.cam = svCam;
                bt.CtrlMode = m_CtrlMode;
                bt.ShiftMode = m_ShiftMode;
                bt.brushSize = m_brushSize;
                bt.brushScreenDir = (svCam.transform.right * Event.current.delta.x - svCam.transform.up * Event.current.delta.y).normalized;
                bt.brushFalloff = m_brushFalloff;
                bt.brushIntensity = m_brushIntensity;
                if (m_currentTool == ePaintingTool.ADD)
                {                   

                    Vector3 worldPos = Vector3.zero;
                    bool collisionFound = false;
                    RaycastHit[] hitInfos = Physics.RaycastAll(r);
                    foreach (RaycastHit hitInfo in hitInfos)
                    {
                        if (hitInfo.collider.gameObject != HairDesignerEditor.m_meshCollider.gameObject)                        
                            continue;//get only our custom collider
                        worldPos = hitInfo.point;                        
                        if (!Event.current.alt && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                            m_lastHairPos = worldPos;

                        dt.normal = hd.transform.InverseTransformDirection(hitInfo.normal);
                        dt.rotation = Quaternion.FromToRotation(Vector3.forward, hd.transform.InverseTransformDirection(hitInfo.normal));
                        bt.worldNormal = hitInfo.normal;
                        dt.meshTriId = hitInfo.triangleIndex;
                        collisionFound = true;

                    }

                    if (collisionFound && !Event.current.alt && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                    {
                        Undo.RecordObject(hd.generator, "hairDesigner brush");
                        dt.localpos = hd.transform.InverseTransformPoint(worldPos);
                        

                        bt.brushDir = hd.transform.InverseTransformDirection(worldPos - m_lastHairPos).normalized;
                        bt.collider = HairDesignerEditor.m_meshCollider;
                        bt.colliderTriangles = HairDesignerEditor.m_meshColliderTriangles;
                        bt.colliderVertices = HairDesignerEditor.m_meshColliderVertices;


                        hd.generator.PaintTool(dt, bt);
                        hd.generator.m_hair = null;
                        //m_needUndo = true;
                    }
                }
            }






            /// <summary>
            /// Generate Cloth Physics
            /// </summary>
            /// <param name="g"></param>
            public void GenerateClothPhysics( HairDesignerGeneratorLongHairBase g )
            {
                if (g.m_meshInstance == null)
                    return;

                EditorUtility.DisplayProgressBar("Compute cloth weights", "Please wait", 0f);
                float progress = 0f;
                bool computeCloth = true;

                

                if (g.m_meshInstance.GetComponent<Cloth>() != null )
                {
                    computeCloth = EditorUtility.DisplayDialog("Cloth component already set", "Replace the cloth component?", "Ok", "Cancel");
                    if (computeCloth)
                        DestroyImmediate(g.m_meshInstance.GetComponent<Cloth>());

                }

                //cloth settings
                if (computeCloth)
                {

                    Cloth clth = g.m_meshInstance.AddComponent<Cloth>();
                    clth.useVirtualParticles = 0;
                    clth.damping = .3f;
                    clth.bendingStiffness = 1f;
                    clth.worldAccelerationScale = .01f;
                    clth.worldVelocityScale = .01f;
                    clth.friction = 1f;
                    //clth.clothSolverFrequency = 120;
                    clth.capsuleColliders = g.m_capsuleColliders.ToArray();
                    //clth.enableContinuousCollision = false;

                    ClothSkinningCoefficient[] coef = clth.coefficients;//  new ClothSkinningCoefficient[m_skinnedMesh.sharedMesh.vertexCount];
                    Vector3[] Mvtc = g.m_skinnedMesh.sharedMesh.vertices;
                    Vector3[] Cvtc = clth.vertices;
                    Vector2[] uv = g.m_skinnedMesh.sharedMesh.uv;

                    for (int c = 0; c < coef.Length; ++c)
                    {
                        float distMin = float.MaxValue;
                        int id = -1;
                        for (int v = 0; v < Mvtc.Length; ++v)
                        {
                            float dist = Vector3.Distance(g.m_skinnedMesh.transform.TransformPoint(Mvtc[v]), g.m_skinnedMesh.rootBone.transform.TransformPoint(Cvtc[c] * .1f));
                            if (dist < distMin)
                            {
                                distMin = dist;
                                id = v;
                                if (dist == 0)
                                    break;
                            }
                        }
                        //coef[c].maxDistance = uv[id].y * .15f;
                        coef[c].maxDistance = uv[id].y * .01f;

                        float f = (float)c / (float)coef.Length;
                        if (f >= progress + .05f)
                        {
                            progress = f;
                            //EditorUtility.DisplayProgressBar("Compute cloth weights", "Please wait", progress);
                            if (EditorUtility.DisplayCancelableProgressBar("Compute cloth weights", "Please wait", progress))
                            {
                                DestroyImmediate(g.m_meshInstance.GetComponent<Cloth>());
                                EditorUtility.ClearProgressBar();
                                return;
                            }

                        }
                    }

                    clth.coefficients = coef;


                    
                }

                EditorUtility.ClearProgressBar();
            }



            public override void DeleteEditorLayer( int idx)
            {
                base.DeleteEditorLayer(idx);
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                bool isEmpty = true;
                for (int i=0; i<g.m_groups.Count; ++i)
                {
                    if(g.m_groups[i].m_layer == idx)
                    {
                        isEmpty = false;
                        break;
                    }
                }

                bool delete = true;
                if( !isEmpty )
                {
                    delete = EditorUtility.DisplayDialog("Delete layer", "Remove all the curves defined in layer "+(idx+1)+" ?", "ok", "cancel");
                }
                else
                {
                    delete = EditorUtility.DisplayDialog("Delete layer", "Remove layer" + (idx + 1) + " ?", "ok", "cancel");
                }

                if( delete )
                {
                    Undo.RecordObject(g, "Delete layer");//save generator for undo

                    for (int i = 0; i < g.m_groups.Count; ++i)
                    {
                        if (g.m_groups[i].m_layer == idx)
                        {
                            g.m_groups.RemoveAt(i--);
                            continue;
                        }

                        if (g.m_groups[i].m_layer > idx)                        
                            g.m_groups[i].m_layer--;//update layer idx                        
                    }

                    g.m_editorLayers.RemoveAt(idx);
                }
            }



            //-------------------------------------------------------------

            public void DrawModifierSceneTool()
            {
                //HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                /*
                for (int i=0; i<g.m_mirrorModifiers.Count; ++i)
                {                    
                    DrawMirrorModifierSceneTool(g.m_mirrorModifiers[i]);
                }*/
            }

            /*
            public void DrawMirrorModifierSceneTool( HairDesignerGeneratorLongHairBase.MirrorModifier m )
            {
                if (m == null) return;

                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                m.pos = g.transform.InverseTransformPoint( Handles.PositionHandle(g.transform.TransformPoint(m.pos), Quaternion.identity) );
            }
            */



                /// <summary>
                /// Snap the hairGrpup to the nearest surcace point
                /// </summary>
                /// <param name="g"></param>
                /// <param name="trans"></param>
                /// <param name="hg"></param>
                /// <returns></returns>
            bool SnapToSurface( HairDesignerGeneratorLongHairBase g, Transform trans, HairDesignerGeneratorLongHairBase.HairGroup hg )
            {
                //snap to surface
                Vector3 rayDir = -hg.m_mCurv.startTangent;

                
                if (hg.m_triLock != null && hg.m_triLock.m_cdata.worldNormalFace.magnitude>0)
                {
                    //snap direction based on previous triangle normal
                    rayDir = trans.InverseTransformDirection(-hg.m_triLock.m_cdata.worldNormalFace);
                    rayDir.Normalize();
                }

                Ray r = new Ray(trans.TransformPoint(hg.m_mCurv.GetPosition(0,0f) - rayDir.normalized * .1f), trans.TransformDirection(rayDir.normalized*.2f));

                RaycastHit[] hits = Physics.RaycastAll(r);
                for (int h = 0; h < hits.Length; ++h)
                {
                    if (hits[h].collider == HairDesignerEditor.m_meshCollider)
                    {                        
                        if (g.m_skinningVersion160)
                        {
                            if (hg.m_triLock == null)
                                hg.m_triLock = new TriangleLock();
                                                        
                            hg.m_mCurv.startPosition = trans.InverseTransformPoint(hits[h].point);
                            hg.m_triLock.Lock(hg.m_mCurv.startPosition, hg.m_mCurv.startTangent, hg.m_mCurv.GetUp(0), HairDesignerEditor.m_meshCollider.transform, g.m_hd.transform, hits[h].triangleIndex, HairDesignerEditor.m_meshColliderVertices, HairDesignerEditor.m_meshColliderTriangles, false);
                            
                            hg.m_mCurv.ConvertOffsetAndRotation(hg.m_triLock.m_cdata.localPosition, hg.m_triLock.m_cdata.localRotation, false);
                            
                        }
                        else
                        {
                            hg.m_mCurv.startPosition = trans.InverseTransformPoint(hits[h].point);
                        }
                        return true;
                    }
                }
                return false;
            }





            /// <summary>
            /// Mirror modifier UI
            /// </summary>
            /// <param name="m"></param>
            public void DrawMirrorModifierUI(HairDesignerGeneratorLongHairBase.MirrorModifier m)
            {
                if (m == null) return;
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                EditorGUI.BeginChangeCheck();

                m.m_axis = (HairDesignerGeneratorLongHairBase.MirrorModifier.eMirrorAxis)EditorGUILayout.EnumPopup("Axis", m.m_axis);
                m.m_offset = EditorGUILayout.Vector3Field("Offset", m.m_offset);
                GUILayout.Label("Layers");
                GUILayout.BeginHorizontal();
                
                for (int l = 0; l < g.m_editorLayers.Count; ++l)
                {
                    GUIStyle s = EditorStyles.miniButtonMid;
                    if( l==0 ) s = EditorStyles.miniButtonLeft;
                    if (l == g.m_editorLayers.Count-1) s = EditorStyles.miniButtonRight;

                    bool layerEnable = m.layers.Contains(l);
                    GUI.color = layerEnable ? Color.white : Color.grey;
                    if (GUILayout.Button("" + (l + 1), s ))
                    {
                        if (layerEnable)
                            m.layers.Remove(l);
                        else
                            m.layers.Add(l);
                    }
                    GUI.color = Color.white;
                }
                if(EditorGUI.EndChangeCheck() )
                    m.Update(g);
                GUILayout.EndHorizontal();

                m.autoUpdate = EditorGUILayout.ToggleLeft("auto update ( low poly mode )", m.autoUpdate);

                GUILayout.BeginHorizontal();

                if (m != null && GUILayout.Button("Update"))
                    m.Update(g);

                if (m != null && GUILayout.Button("Apply"))
                {
                    if (EditorUtility.DisplayDialog("Apply modifier", "The mirror modifier will be destroyed and the hair strands will become selectable.", "Ok", "Cancel"))
                    {
                        m.Apply(g);
                        g.m_mirrorModifiers.Remove(m);
                    }
                }
                GUILayout.EndHorizontal();
            }


            public override void OnLayerChange()
            {
                HairDesignerGeneratorLongHairBase g = target as HairDesignerGeneratorLongHairBase;
                if (!m_FastDraw)
                    g.m_needMeshRebuild = true;
                //DestroyImmediate(g.m_meshInstance);
                //g.GenerateMeshRenderer();
            }


            public void ReplaceBaseScripts(HairDesignerGeneratorLongHairBase g)
            {
                //return;
                
                g.m_meshInstanceRef = HairDesignerEditor.ReplaceBaseClass(g.m_meshInstanceRef) as HairDesignerMeshInstanceBase;

                HairDesignerBoneBase[] bonesRoots = g.GetComponentsInChildren<HairDesignerBoneBase>(true);                             
                
                for (int i = 0; i < bonesRoots.Length; ++i)
                {
                    bonesRoots[i] = HairDesignerEditor.ReplaceBaseClass(bonesRoots[i]) as HairDesignerBoneBase;
                    bonesRoots[i].m_meshReference = g.m_meshInstanceRef;
                }
                
                /*
                for(int i=0; i<g.m_groups.Count; ++i)
                {
                    g.m_groups[i].m_
                }
                */
                
            }

    }

        
    }
}