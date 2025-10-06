using UnityEngine;

namespace Game.Runtime.Investigation.Data
{
    [CreateAssetMenu(fileName = "New Character", menuName = "Game/Investigation/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("Basic Info")]
        public string characterId;
        public string fullName;
        public int age;
        public Sprite profilePicture;
        
        [Header("Contact Info")]
        public string emailAddress;
        public string phoneNumber;
        public string socialMediaHandle;
        
        [Header("Background")]
        [TextArea(3, 10)]
        public string biography;
        public string occupation;
        public string location;
        
        [Header("Relationships")]
        public string[] knownAssociates;
        
        [Header("Investigation")]
        public bool isSuspect;
        public string[] aliases;
    }
}