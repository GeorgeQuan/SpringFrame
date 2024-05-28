using UnityEngine;

namespace Koakuma.Game
{
    public static class LayerMaskUtility
    {
        public static int ALL => -1;

        private static int? defaultLayer;
        public static int DEFAULT_LAYER
        {
            get
            {
                if (!defaultLayer.HasValue)
                {
                    defaultLayer = LayerMask.NameToLayer("Default");
                }
                return defaultLayer.Value;
            }
        }

        private static int? playerLayer;
        public static int PLAYER_LAYER
        {
            get
            {
                if (!playerLayer.HasValue)
                {
                    playerLayer = LayerMask.NameToLayer("Player");
                }
                return playerLayer.Value;
            }
        }

        private static int? monsterLayer;
        public static int MONSTER_LAYER
        {
            get
            {
                if (!monsterLayer.HasValue)
                {
                    monsterLayer = LayerMask.NameToLayer("Monster");
                }
                return monsterLayer.Value;
            }
        }

        private static int? ragdollLayer;
        public static int RAGDOLL_LAYER
        {
            get
            {
                if (!ragdollLayer.HasValue)
                {
                    ragdollLayer = LayerMask.NameToLayer("Ragdoll");
                }
                return ragdollLayer.Value;
            }
        }

        private static int? bulletLayer;
        public static int BULLET_LAYER
        {
            get
            {
                if (!bulletLayer.HasValue)
                {
                    bulletLayer = LayerMask.NameToLayer("Bullet");
                }
                return bulletLayer.Value;
            }
        }

        private static int? spellFieldLayer;
        public static int SPELL_FIELD_LAYER
        {
            get
            {
                if (!spellFieldLayer.HasValue)
                {
                    spellFieldLayer = LayerMask.NameToLayer("SpellField");
                }
                return spellFieldLayer.Value;
            }
        }

        private static int? wallLayer;
        public static int WALL_LAYER
        {
            get
            {
                if (!wallLayer.HasValue)
                {
                    wallLayer = LayerMask.NameToLayer("Wall");
                }
                return wallLayer.Value;
            }
        }

        private static int? terrainLayer;
        public static int TERRAIN_LAYER
        {
            get
            {
                if (!terrainLayer.HasValue)
                {
                    terrainLayer = LayerMask.NameToLayer("Terrain");
                }
                return terrainLayer.Value;
            }
        }

        private static int? objectLayer;
        public static int OBJECT_LAYER
        {
            get
            {
                if (!objectLayer.HasValue)
                {
                    objectLayer = LayerMask.NameToLayer("Object");
                }
                return objectLayer.Value;
            }
        }

        private static int? invisibleLayer;
        public static int INVISIBLE_LAYER
        {
            get
            {
                if (!invisibleLayer.HasValue)
                {
                    invisibleLayer = LayerMask.NameToLayer("Invisible");
                }
                return invisibleLayer.Value;
            }
        }

        private static int? invisibleCharacterLayer;
        public static int INVISIBLE_CHARACTER_LAYER
        {
            get
            {
                if (!invisibleCharacterLayer.HasValue)
                {
                    invisibleCharacterLayer = LayerMask.NameToLayer("InvisibleCharacter");
                }
                return invisibleCharacterLayer.Value;
            }
        }

        private static int? interactiveObjectLayer;
        public static int INTERACTIVE_OBJECT_LAYER
        {
            get
            {
                if (!interactiveObjectLayer.HasValue)
                {
                    interactiveObjectLayer = LayerMask.NameToLayer("InteractiveObject");
                }
                return interactiveObjectLayer.Value;
            }
        }

        private static int? vCamLayer;
        public static int VCamLayer
        {
            get
            {
                if (!vCamLayer.HasValue)
                {
                    vCamLayer = LayerMask.NameToLayer("VCamera");
                }
                return vCamLayer.Value;
            }
        }

        private static int? maskLayer;
        public static int MaskLayer
        {
            get
            {
                if (!maskLayer.HasValue)
                {
                    maskLayer = LayerMask.NameToLayer("Mask");
                }
                return maskLayer.Value;
            }
        }
    }
}
