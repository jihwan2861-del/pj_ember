// Copy into Assets/Editor. Unity 6, no additional packages required.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class UnityWebRigImporter
{
    [Serializable] public class Pose { public string id; public float x,y,rotation,scaleX=1,scaleY=1; }
    [Serializable] public class Part { public string id,name,parent,source; public float pivotX=.5f,pivotY=.5f; public int order; public Pose bind; }
    [Serializable] public class Key { public int frame; public Pose[] transforms; }
    [Serializable] public class Rig { public int version,length,fps; public string name; public float ppu; public bool loop; public Part[] parts; public Key[] keys; }

    [MenuItem("Tools/Ember/Import Web Rig")]
    public static void ImportMenu()
    {
        string file=EditorUtility.OpenFilePanel("Import Web Rig", "", "json");
        if(string.IsNullOrEmpty(file))return;
        try { var result=ImportFile(file); Selection.activeObject=result; EditorGUIUtility.PingObject(result); }
        catch(Exception e) { Debug.LogException(e); EditorUtility.DisplayDialog("Web Rig import failed",e.Message,"OK"); }
    }

    public static GameObject ImportFile(string file)
    {
        Rig rig=JsonUtility.FromJson<Rig>(File.ReadAllText(file));
        if(rig==null||rig.version!=1||rig.parts==null||rig.parts.Length<1||rig.parts.Length>100||rig.ppu<1||rig.fps<1||rig.fps>120||rig.length<2||rig.length>600)throw new Exception("Invalid or unsupported rig file.");
        var map=new Dictionary<string,Part>();
        foreach(var p in rig.parts) { if(p==null||string.IsNullOrEmpty(p.id)||map.ContainsKey(p.id)||!System.Text.RegularExpressions.Regex.IsMatch(p.id,"^p[a-zA-Z0-9]+$")||p.bind==null)throw new Exception("Invalid part or duplicate ID.");map.Add(p.id,p); ValidatePose(p.bind); }
        foreach(var p in rig.parts) { var seen=new HashSet<string>();var a=p;while(!string.IsNullOrEmpty(a.parent)) { if(!seen.Add(a.id)||!map.TryGetValue(a.parent,out a))throw new Exception("Invalid parent hierarchy."); } }
        var keys=(rig.keys??new Key[0]).OrderBy(k=>k.frame).ToArray();
        if(keys.Length>600||keys.Select(k=>k.frame).Distinct().Count()!=keys.Length)throw new Exception("Invalid keys.");
        foreach(var k in keys) { if(k.frame<0||k.frame>=rig.length||k.transforms==null)throw new Exception("Key outside timeline.");foreach(var t in k.transforms){if(!map.ContainsKey(t.id))throw new Exception("Unknown part in key.");ValidatePose(t);} }
        // Validate all PNG bytes before writing assets.
        var bytes=new Dictionary<string,byte[]>();
        foreach(var p in rig.parts) { const string prefix="data:image/png;base64,";if(p.source==null||!p.source.StartsWith(prefix))throw new Exception("Parts must contain PNG images. Export again from Rig Studio.");var data=Convert.FromBase64String(p.source.Substring(prefix.Length));if(data.Length<8||data[0]!=137||data[1]!=80||data[2]!=78||data[3]!=71)throw new Exception("Invalid PNG.");bytes[p.id]=data; }
        if(!AssetDatabase.IsValidFolder("Assets/WebRigs"))AssetDatabase.CreateFolder("Assets","WebRigs");
        var safe=new string((rig.name??"WebRig").Where(c=>char.IsLetterOrDigit(c)||c=='_'||c=='-').ToArray());if(string.IsNullOrEmpty(safe))safe="WebRig";
        var folder=AssetDatabase.GenerateUniqueAssetPath("Assets/WebRigs/"+safe);AssetDatabase.CreateFolder("Assets/WebRigs",Path.GetFileName(folder));
        var root=new GameObject(safe);
        try
        {
            var objects=new Dictionary<string,GameObject>();
            foreach(var p in rig.parts)objects[p.id]=new GameObject(p.id);
            foreach(var p in rig.parts)
            {
                var go=objects[p.id];go.transform.SetParent(string.IsNullOrEmpty(p.parent)?root.transform:objects[p.parent].transform,false);
                SetPose(go.transform,p.bind,rig.ppu);
                string path=folder+"/"+p.id+".png";File.WriteAllBytes(path,bytes[p.id]);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=rig.ppu;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=8192;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(Mathf.Clamp01(p.pivotX),1-Mathf.Clamp01(p.pivotY));importer.SetTextureSettings(settings);importer.SaveAndReimport();
                var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);renderer.sortingOrder=p.order;
                // Names use stable IDs so duplicate user labels do not break animation paths.
            }
            var clip=new AnimationClip { name=safe+"_Motion",frameRate=rig.fps };
            foreach(var p in rig.parts)
            {
                string path=AnimationUtility.CalculateTransformPath(objects[p.id].transform,root.transform);
                var frames=keys.Select(k=>k.frame).Concat(new[]{0,rig.length-1}).Distinct().OrderBy(f=>f).ToArray();
                AddCurve(clip,path,"m_LocalPosition.x",frames,f=>Sample(p,keys,f).x/rig.ppu,rig.fps);
                AddCurve(clip,path,"m_LocalPosition.y",frames,f=>Sample(p,keys,f).y/rig.ppu,rig.fps);
                AddCurve(clip,path,"localEulerAnglesRaw.z",frames,f=>Sample(p,keys,f).rotation,rig.fps);
                AddCurve(clip,path,"m_LocalScale.x",frames,f=>Sample(p,keys,f).scaleX,rig.fps);
                AddCurve(clip,path,"m_LocalScale.y",frames,f=>Sample(p,keys,f).scaleY,rig.fps);
            }
            var clipSettings=AnimationUtility.GetAnimationClipSettings(clip);clipSettings.loopTime=rig.loop;AnimationUtility.SetAnimationClipSettings(clip,clipSettings);AssetDatabase.CreateAsset(clip,folder+"/"+safe+".anim");
            var controller=AnimatorController.CreateAnimatorControllerAtPath(folder+"/"+safe+".controller");var state=controller.layers[0].stateMachine.AddState("Motion");state.motion=clip;controller.layers[0].stateMachine.defaultState=state;root.AddComponent<Animator>().runtimeAnimatorController=controller;
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+"/"+safe+".prefab");File.WriteAllText(folder+"/PartNames.txt",string.Join("\n",rig.parts.Select(p=>p.id+" = "+p.name)));AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Web Rig imported: "+folder);return prefab;
        }
        catch { AssetDatabase.DeleteAsset(folder);throw; }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
    static void ValidatePose(Pose p) { if(p==null||new[]{p.x,p.y,p.rotation,p.scaleX,p.scaleY}.Any(v=>float.IsNaN(v)||float.IsInfinity(v))||p.scaleX<=0||p.scaleY<=0)throw new Exception("Invalid pose values."); }
    static void SetPose(Transform t,Pose p,float ppu) { t.localPosition=new Vector3(p.x/ppu,p.y/ppu,0);t.localRotation=Quaternion.Euler(0,0,p.rotation);t.localScale=new Vector3(p.scaleX,p.scaleY,1); }
    static Pose Sample(Part part,Key[] keys,int f)
    {
        if(keys.Length==0)return part.bind;Key a=keys[0],b=keys[keys.Length-1];if(f<=a.frame)b=a;else if(f>=b.frame)a=b;else for(int i=0;i<keys.Length-1;i++)if(f>=keys[i].frame&&f<=keys[i+1].frame){a=keys[i];b=keys[i+1];break;}
        var aa=a.transforms.FirstOrDefault(t=>t.id==part.id)??part.bind;var bb=b.transforms.FirstOrDefault(t=>t.id==part.id)??part.bind;float v=a==b?0:(float)(f-a.frame)/(b.frame-a.frame);
        return new Pose{x=Mathf.Lerp(aa.x,bb.x,v),y=Mathf.Lerp(aa.y,bb.y,v),rotation=Mathf.Lerp(aa.rotation,bb.rotation,v),scaleX=Mathf.Lerp(aa.scaleX,bb.scaleX,v),scaleY=Mathf.Lerp(aa.scaleY,bb.scaleY,v)};
    }
    static void AddCurve(AnimationClip clip,string path,string property,int[] frames,Func<int,float> value,int fps)
    {
        var curve=new AnimationCurve(frames.Select(f=>new Keyframe((float)f/fps,value(f))).ToArray());for(int i=0;i<curve.length;i++){AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.Linear);}clip.SetCurve(path,typeof(Transform),property,curve);
    }
}
#endif
