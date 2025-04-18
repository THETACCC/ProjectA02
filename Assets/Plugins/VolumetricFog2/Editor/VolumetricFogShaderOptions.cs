﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace VolumetricFogAndMist2 {

    public class VolumetricFogShaderOptions {

        const string SHADER_NAME = "VolumetricFog2/VolumetricFog2DURP";
        const string OPTIONS_SHADER_FILENAME = "CommonsURP.hlsl";
        const string OPTIONS_VOID_MANAGER_FILENAME = "VolumetricFogManager.cs";
        const string OPTIONS_FOG_SCRIPT_FILENAME = "VolumetricFog.cs";
        const string OPTIONS_FOG_EDITOR_SCRIPT_FILENAME = "VolumetricFogProfileEditor.cs";

        public bool pendingChanges;
        public ShaderAdvancedOption[] options;

        public void ReadOptions() {
            pendingChanges = false;
            // Populate known options
            options = new ShaderAdvancedOption[]
            {
                new ShaderAdvancedOption
                {
                    id = "ORTHO_SUPPORT", name = "Orthographic Mode", description = "Enables support for orthographic camera projection."
                },
                new ShaderAdvancedOption
                {
                    id = "USE_ALTERNATE_RECONSTRUCT_API",
                    name = "Alternate WS Reconstruction",
                    description = "Uses an alternate world space position reconstruction in XR."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_BLUE_NOISE",
                    name = "Blue Noise",
                    description = "Uses blue noise based dithering/jittering."
                },
                new ShaderAdvancedOption
                {
                    id = "USE_WORLD_SPACE_NOISE",
                    name = "World Space Noise",
                    description = "Uses world-space aligned noise (noise will change when fog volume position changes)."
                },
                new ShaderAdvancedOption
                {
                    id = "WEBGL_COMPATIBILITY_MODE",
                    name = "WebGL Compatibilty Mode",
                    description = "Enable this option only if you're building for WebGL and notice rendering issues."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_VOID_ROTATION",
                    name = "Voids Rotation",
                    description = "Enable this option to allow rotation of fog voids using the transform."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_ROTATION",
                    name = "Volume Rotation",
                    description = "Enable this option to allow rotation of fog volumes using the transform."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_BORDER",
                    name = "Volume Border Option",
                    description = "Enables the 'border' option in the profile which adds a falloff to the edges of the volume."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_MAX_DISTANCE_XZ",
                    name = "Max Distance on X/Z",
                    description = "Ignores Y-axis when applying the max distance option."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_SHADOW_CANCELLATION",
                    name = "Shadow Fog Cancellation Option",
                    description = "Enables the 'shadow cancellation' option in the profile which reduces the fog density or makes it invisible when in shadows."
                },
                new ShaderAdvancedOption
                {
                    id = "V2F_LIGHT_COOKIE_CANCELLATION",
                    name = "Cookie Fog Cancellation",
                    description = "Changes fog transparency depending on the cookie color which acts as an alpha mask."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_FORWARD_PLUS_IGNORE_CLUSTERING",
                    name = "Ignore clustering in Forward+",
                    description = "Parses all lights per pixel in Forward+, ignoring clustering (only available in Unity 2022/URP 14). In forward+, lights are grouped in clusters (screen partitions) to reduce the amount of lights computed per pixel. However, if there're few lights visible (less than 8), it may be faster to disable clustering by enabling this option."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_FORWARD_PLUS_ADDITIONAL_DIRECTIONAL_LIGHTS",
                    name = "Additional Directional Lights in Forward+",
                    description = "Supports extra directional lights in Forward+. By default, this option is disabled to improve performance."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_LIGHT_LAYERS",
                    name = "Rendering/Light Layers Support",
                    description = "Supports culling lights by rendering layers. By default, this option is disabled to improve performance."
                },
                new ShaderAdvancedOption
                {
                    id = "MAX_ITERATIONS",
                    name = "",
                    description = "",
                    hasValue = true
                }
            };


            Shader shader = Shader.Find(SHADER_NAME);
            if (shader != null) {
                string path = AssetDatabase.GetAssetPath(shader);
                string file = Path.GetDirectoryName(path) + "/" + OPTIONS_SHADER_FILENAME;
                string[] lines = File.ReadAllLines(file, Encoding.UTF8);
                for (int k = 0; k < lines.Length; k++) {
                    for (int o = 0; o < options.Length; o++) {
                        if (lines[k].Contains("#define " + options[o].id)) {
                            options[o].enabled = !lines[k].StartsWith("//");
                            if (options[o].hasValue) {
                                string[] tokens = lines[k].Split(null);
                                if (tokens.Length > 2) {
                                    int.TryParse(tokens[2], out options[o].value);
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }


        public bool GetAdvancedOptionState(string optionId) {
            if (options == null)
                return false;
            for (int k = 0; k < options.Length; k++) {
                if (options[k].id.Equals(optionId)) {
                    return options[k].enabled;
                }
            }
            return false;
        }

        public void UpdateAdvancedOptionsFile() {
            // Reloads the file and updates it accordingly
            Shader shader = Shader.Find(SHADER_NAME);
            if (shader != null) {
                string path = AssetDatabase.GetAssetPath(shader);

                // update shader options
                string file = Path.GetDirectoryName(path) + "/" + OPTIONS_SHADER_FILENAME;
                UpdateOptionsFile(file);

                // update void manager options
                file = Path.GetDirectoryName(path) + "/../../Scripts/Managers/" + OPTIONS_VOID_MANAGER_FILENAME;
                UpdateOptionsFile(file);

                // update main fog script options
                file = Path.GetDirectoryName(path) + "/../../Scripts/" + OPTIONS_FOG_SCRIPT_FILENAME;
                UpdateOptionsFile(file);

                // update editor script options
                file = Path.GetDirectoryName(path) + "/../../Editor/" + OPTIONS_FOG_EDITOR_SCRIPT_FILENAME;
                UpdateOptionsFile(file);
            }

            pendingChanges = false;
            AssetDatabase.Refresh();
        }

        void UpdateOptionsFile(string file) {
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);
            for (int k = 0; k < lines.Length; k++) {
                for (int o = 0; o < options.Length; o++) {
                    string token = "#define " + options[o].id;
                    if (lines[k].Contains(token)) {
                        if (options[o].hasValue) {
                            lines[k] = token + " " + options[o].value;
                        } else {
                            if (options[o].enabled) {
                                lines[k] = token;
                            } else {
                                lines[k] = "//#define " + options[o].id;
                            }
                        }
                        break;
                    }
                }
            }
            File.WriteAllLines(file, lines, Encoding.UTF8);
        }

        public int GetOptionValue(string id) {
            for (int k = 0; k < options.Length; k++) {
                if (options[k].hasValue && options[k].id.Equals(id)) {
                    return options[k].value;
                }
            }
            return 0;
        }

        public void SetOptionValue(string id, int value) {
            for (int k = 0; k < options.Length; k++) {
                if (options[k].hasValue && options[k].id.Equals(id)) {
                    options[k].value = value;
                }
            }
        }


    }

    public struct ShaderAdvancedOption {
        public string id;
        public string name;
        public string description;
        public bool enabled;
        public bool hasValue;
        public int value;
    }


}