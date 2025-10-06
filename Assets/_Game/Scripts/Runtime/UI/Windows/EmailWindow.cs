using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Game.Runtime.UI.Windows
{
    public class EmailWindow : WindowBase
    {
        [Header("Email Components")]
        [SerializeField] private Transform emailListContainer;
        [SerializeField] private TextMeshProUGUI emailContentText;
        [SerializeField] private TextMeshProUGUI senderText;
        [SerializeField] private TextMeshProUGUI subjectText;
        
        private List<EmailData> _emails = new List<EmailData>();
        
        protected override void Awake()
        {
            base.Awake();
            windowTitle = "Email Client";
        }
        
        protected override void OnOpen()
        {
            base.OnOpen();
            LoadEmails();
        }
        
        private void LoadEmails()
        {
            _emails.Clear();
            
            _emails.Add(new EmailData
            {
                Id = "email_001",
                Sender = "john.doe@example.com",
                Subject = "Important: Project Update",
                Content = "Hi there,\n\nPlease check the attached documents...",
                IsRead = false
            });
        }
    }
    
    [System.Serializable]
    public class EmailData
    {
        public string Id;
        public string Sender;
        public string Subject;
        public string Content;
        public bool IsRead;
        public List<string> Attachments;
    }
}