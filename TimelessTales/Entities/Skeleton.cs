using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TimelessTales.Entities
{
    /// <summary>
    /// Manages a hierarchy of bones for skeletal animation
    /// </summary>
    public class Skeleton
    {
        private readonly Dictionary<string, Bone> _bones = new();
        private readonly List<Bone> _rootBones = new();
        
        public Bone AddBone(string name, Vector3 localPosition, Bone? parent = null)
        {
            Bone bone = new Bone(name, localPosition)
            {
                Parent = parent
            };
            
            _bones[name] = bone;
            
            if (parent == null)
            {
                _rootBones.Add(bone);
            }
            
            bone.UpdateTransforms();
            return bone;
        }
        
        public Bone? GetBone(string name)
        {
            return _bones.TryGetValue(name, out var bone) ? bone : null;
        }
        
        public void UpdateAllTransforms()
        {
            // Update root bones first, then their children will be updated recursively
            foreach (var root in _rootBones)
            {
                UpdateBoneHierarchy(root);
            }
        }
        
        private void UpdateBoneHierarchy(Bone bone)
        {
            bone.UpdateTransforms();
            
            // Update children
            foreach (var childBone in _bones.Values)
            {
                if (childBone.Parent == bone)
                {
                    UpdateBoneHierarchy(childBone);
                }
            }
        }
        
        public IEnumerable<Bone> GetAllBones()
        {
            return _bones.Values;
        }
    }
}
