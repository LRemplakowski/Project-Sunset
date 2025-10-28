using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderCrew.SeeThroughShader
{
    public static class SeeThroughShaderConstants
    {
        //public static readonly int NUMBER_OF_PROPERTIES_PLAYERDATA = 6;
        public static readonly string PLAYERS_POS_SHADER_PROPERTY_NAME = "_PlayersPosVectorArray";
        public static readonly string PLAYERS_DATA_SHADER_PROPERTY_NAME = "_PlayersDataFloatArray";
        public static readonly string PLAYERS_POS_ARRAY_COUNT_PROPERTY_NAME = "_ArrayLength";

#if (UNITY_WEBGL)
        public static readonly int PLAYERS_POS_ARRAY_LENGTH = 20;
        public static readonly int PLAYERS_DATA_ARRAY_LENGTH = 150;
        public static readonly int ZONES_DATA_ARRAY_LENGTH = 500;   
#else
        public static readonly int PLAYERS_POS_ARRAY_LENGTH = 100;
        public static readonly int PLAYERS_DATA_ARRAY_LENGTH = 500;
        public static readonly int ZONES_DATA_ARRAY_LENGTH = 1000;
#endif
        public static readonly string ZONES_DATA_SHADER_PROPERTY_NAME = "_ZDFA"; // _ZonesDataFloatArray



        public static readonly string ZONES_DATA_COUNT_SHADER_PROPERTY_NAME = "_ZonesDataCount";

        public static readonly string STS_INSTANCE_PREFIX = "STS Instance"; // of";
        public static readonly string STS_TRIGGER_PREFIX = "STS Trigger"; // of";

        public static readonly string STS_SYNCHRONIZATION_PREFIX = "Synchronized with";


        // PROPERTIES
        public static readonly string PROPERTY_GLOBAL = "Global";
        public static readonly string PROPERTY_IS_REPLACEMENT_SHADER = "_IsReplacementShader";
        public static readonly string PROPERTY_DISSOLVE_TEX = "_DissolveTex";
        public static readonly string PROPERTY_DISSOLVE_TEX_GLOBAL = PROPERTY_DISSOLVE_TEX + PROPERTY_GLOBAL;
        public static readonly string PROPERTY_DISSOLVE_MASK = "_DissolveMask";
        public static readonly string PROPERTY_DISSOLVE_MASK_GLOBAL = PROPERTY_DISSOLVE_MASK + PROPERTY_GLOBAL;
        public static readonly string PROPERTY_OBSTRUCTION_CURVE = "_ObstructionCurve";
        public static readonly string PROPERTY_OBSTRUCTION_CURVE_GLOBAL = PROPERTY_OBSTRUCTION_CURVE + PROPERTY_GLOBAL;
        public static readonly string PROPERTY_DISSOLVE_COLOR = "_DissolveColor";
        public static readonly string PROPERTY_DISSOLVE_COLOR_GLOBAL = PROPERTY_DISSOLVE_COLOR + PROPERTY_GLOBAL;


        public const string PROPERTY_IS_REFERENCE_MATERIAL = "_isReferenceMaterial";
        public const string PROPERTY_NUM_OF_PLAYERS_INSIDE = "_numOfPlayersInside";
        public const string PROPERTY_T_VALUE = "_tValue";
        public const string PROPERTY_T_DIRECTION = "_tDirection";
        public const string PROPERTY_ID = "_id";
        public const string PROPERTY_TRIGGER_MODE = "_TriggerMode";
        public const string PROPERTY_RAYCAST_MODE = "_RaycastMode";
        public const string PROPERTY_IS_EXEMPT = "_IsExempt";

        public const string PROPERTY_SHOW_CONTENT_DISSOLVE_AREA = "_ShowContentDissolveArea";
        public const string PROPERTY_SHOW_CONTENT_INTERACTION_OPTIONS_AREA = "_ShowContentInteractionOptionsArea";
        public const string PROPERTY_SHOW_CONTENT_OBSTRUCTION_OPTIONS_AREA = "_ShowContentObstructionOptionsArea";
        public const string PROPERTY_SHOW_CONTENT_ANIMATION_AREA = "_ShowContentAnimationArea";
        public const string PROPERTY_SHOW_CONTENT_ZONING_AREA = "_ShowContentZoningArea";
        public const string PROPERTY_SHOW_CONTENT_REPLACEMENT_OPTIONS_AREA = "_ShowContentReplacementOptionsArea";
        public const string PROPERTY_SHOW_CONTENT_DEBUG_AREA = "_ShowContentDebugArea";


        public const string STS_SHADER_IDENTIFIER_PROPERTY = "_CurveObstructionDestroyRadius"; //temp
        

        // KEYWORDS
        public static readonly string KEYWORD_REPLACEMENT = "_REPLACEMENT";

        public static readonly string KEYWORD_PLAYERINDEPENDENT = "_PLAYERINDEPENDENT";
        public static readonly string KEYWORD_ZONING = "_ZONING";
        public static readonly string KEYWORD_DISSOLVEMASK = "_DISSOLVEMASK";
        public static readonly string KEYWORD_OBSTRUCTION_CURVE = "_OBSTRUCTION_CURVE";

        //public static readonly string STS_FOLDER = "SeeThroughShader/";

        ////BiRP
        //public static readonly string BIRP_STS_STANDARD = STS_FOLDER + "Standard";

        //public static readonly string STS_UNLIT = STS_FOLDER + "Unlit/";

        //public static readonly string BIRP_STS_UNLIT_COLOR = STS_UNLIT + "Color";
        //public static readonly string BIRP_STS_UNLIT_TEXTURE = STS_UNLIT + "Texture";


        ////HDRP
        //public static readonly string HDRP_STS_LIT = STS_FOLDER + "Lit";

        ////URP
        //public static readonly string URP_STS_LIT = STS_FOLDER + "Lit";

        public static readonly string STS_SHADER_DEFAULT_KEY = "default";









        public static readonly string STS_SHADER_NAME_FOLDER = "SeeThroughShader";
        //public static readonly string STS_SHADER_NAME = "/SeeThroughShader";
        public static readonly string STS_SHADER_NAME_LIT = "/Lit";


        /// BiRP ///
        public static readonly string STS_SHADER_NAME_BIRP_FOLDER = STS_SHADER_NAME_FOLDER + "/BiRP";

        public static readonly string STS_SHADER_NAME_BIRP_STANDARD = STS_SHADER_NAME_BIRP_FOLDER + "/Standard";
        public static readonly string STS_SHADER_NAME_BIRP_UNLIT_COLOR = STS_SHADER_NAME_BIRP_FOLDER + "/Unlit/Color";
        public static readonly string STS_SHADER_NAME_BIRP_UNLIT_TEXTURE = STS_SHADER_NAME_BIRP_FOLDER + "/Unlit/Texture";

        /// URP ///
        public static readonly string STS_SHADER_NAME_URP_FOLDER = STS_SHADER_NAME_FOLDER + "/URP";

        /// URP 2019///
        public static readonly string STS_SHADER_NAME_URP_2019 = STS_SHADER_NAME_URP_FOLDER + "/2019" + STS_SHADER_NAME_LIT;
        /// URP 2020///
        public static readonly string STS_SHADER_NAME_URP_2020 = STS_SHADER_NAME_URP_FOLDER + "/2020" + STS_SHADER_NAME_LIT;
        /// URP 2021///
        public static readonly string STS_SHADER_NAME_URP_2021 = STS_SHADER_NAME_URP_FOLDER + "/2021" + STS_SHADER_NAME_LIT;
        /// URP 2022///
        public static readonly string STS_SHADER_NAME_URP_2022 = STS_SHADER_NAME_URP_FOLDER + "/2022" + STS_SHADER_NAME_LIT;
        /// URP 6///
        public static readonly string STS_SHADER_NAME_URP_UNITY6 = STS_SHADER_NAME_URP_FOLDER + "/Unity6" + STS_SHADER_NAME_LIT;


        /// HDRP ///
        public static readonly string STS_SHADER_NAME_HDRP_FOLDER = STS_SHADER_NAME_FOLDER + "/HDRP";

        /// HDRP 2019///
        public static readonly string STS_SHADER_NAME_HDRP_2019 = STS_SHADER_NAME_HDRP_FOLDER + "/2019" + STS_SHADER_NAME_LIT;
        /// HDRP 2020///
        public static readonly string STS_SHADER_NAME_HDRP_2020 = STS_SHADER_NAME_HDRP_FOLDER + "/2020" + STS_SHADER_NAME_LIT;
        /// HDRP 2021///
        public static readonly string STS_SHADER_NAME_HDRP_2021 = STS_SHADER_NAME_HDRP_FOLDER + "/2021" + STS_SHADER_NAME_LIT;
        /// HDRP 2022///
        public static readonly string STS_SHADER_NAME_HDRP_2022 = STS_SHADER_NAME_HDRP_FOLDER + "/2022" + STS_SHADER_NAME_LIT;
        /// HDRP 6///
        public static readonly string STS_SHADER_NAME_HDRP_UNITY6 = STS_SHADER_NAME_HDRP_FOLDER + "/Unity6" + STS_SHADER_NAME_LIT;



        /// BiRP ///
        public static readonly string STS_SHADER_FILENAME_BIRP_STANDARD = "/BiRP(Built-in)" + "/SeeThroughShader_Standard_BiRP";
        public static readonly string STS_SHADER_FILENAME_BIRP_UNLIT_COLOR = "/BiRP(Built-in)" + "/SeeThroughShader_UnlitColor_BiRP";
        public static readonly string STS_SHADER_FILENAME_BIRP_UNLIT_TEXTURE = "/BiRP(Built-in)" + "/SeeThroughShader_UnlitTexture_BiRP";

        /// URP 2019///
        public static readonly string STS_SHADER_FILENAME_URP_2019 = "/URP" + "/2019" + "/SeeThroughShader" + "_Lit" + "_URP2019";
        /// URP 2020///
        public static readonly string STS_SHADER_FILENAME_URP_2020 = "/URP" + "/2020" + "/SeeThroughShader" + "_Lit" + "_URP2020";
        /// URP 2021///
        public static readonly string STS_SHADER_FILENAME_URP_2021 = "/URP" + "/2021" + "/SeeThroughShader" + "_Lit" + "_URP2021";
        /// URP 2022///
        public static readonly string STS_SHADER_FILENAME_URP_2022 = "/URP" + "/2022" + "/SeeThroughShader" + "_Lit" + "_URP2022";
        /// URP 6///
        public static readonly string STS_SHADER_FILENAME_URP_UNITY6 = "/URP" + "/Unity6" + "/SeeThroughShader" + "_Lit" + "_URPUnity6";


        /// HDRP 2019///
        public static readonly string STS_SHADER_FILENAME_HDRP_2019 = "/HDRP" + "/2019" + "/SeeThroughShader" + "_Lit" + "_HDRP2019";
        /// HDRP 2020///
        public static readonly string STS_SHADER_FILENAME_HDRP_2020 = "/HDRP" + "/2020" + "/SeeThroughShader" + "_Lit" + "_HDRP2020";
        /// HDRP 2021///
        public static readonly string STS_SHADER_FILENAME_HDRP_2021 = "/HDRP" + "/2021" + "/SeeThroughShader" + "_Lit" + "_HDRP2021";
        /// HDRP 2022///
        public static readonly string STS_SHADER_FILENAME_HDRP_2022 = "/HDRP" + "/2022" + "/SeeThroughShader" + "_Lit" + "_HDRP2022";
        /// HDRP 6///
        public static readonly string STS_SHADER_FILENAME_HDRP_UNITY6 = "/HDRP" + "/Unity6" + "/SeeThroughShader" + "_Lit" + "_HDRPUnity6";



    }
}