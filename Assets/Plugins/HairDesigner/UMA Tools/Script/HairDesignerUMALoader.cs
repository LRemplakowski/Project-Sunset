using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kalagaan.HairDesignerExtension;

public class HairDesignerUMALoader : MonoBehaviour {

    public List<HairDesignerRuntimeLayer> m_runtimeLayers = new List<HairDesignerRuntimeLayer>();
    [HideInInspector]
    public HairDesigner m_hairDesigner = null;

    public void Awake()
    {
                
        UMA.UMAAvatarBase avatar = GetComponent<UMA.UMAAvatarBase>();
        if (avatar != null)
            avatar.CharacterCreated.AddListener(GenerateLayers);
        
    }



    void GenerateLayers(UMA.UMAData data)
    {
        m_hairDesigner = data.GetRenderer(0).gameObject.AddComponent<HairDesigner>();
        HairDesignerRuntimeLayer.m_boneNameTotransform.Clear();

        for (int l = 0; l < m_runtimeLayers.Count; ++l)
        {
            HairDesignerRuntimeLayer rtl = m_runtimeLayers[l];

            if (rtl == null)
                continue;

            for (int i = 0; i < rtl.m_requiredBones.Count; ++i)
            {
                GameObject bone = data.GetBoneGameObject(rtl.m_requiredBones[i]);
                if (bone != null)
                    HairDesignerRuntimeLayer.m_boneNameTotransform.Add(rtl.m_requiredBones[i], bone.transform);
            }

            
            rtl.GenerateLayers(m_hairDesigner);            
        }

        HairDesignerRuntimeLayer.m_boneNameTotransform.Clear();

    }


}
