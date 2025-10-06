using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;

namespace Game.Runtime.UI.Windows
{
    /// <summary>
    /// Base class for all window components
    /// Follows SOLID principles with proper separation of concerns
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class WindowBase : MonoBehaviour, IWindow, IPointerDownHandler
    {
        [Header("Window Settings")]
        [SerializeField] protected string windowId;
        [SerializeField] protected string windowTitle = "Window";
        [SerializeField] protected Vector2 minSize = new Vector2(400, 300);
        [SerializeField] protected Vector2 maxSize = new Vector2(1920, 1080);
        [SerializeField] protected bool canResize = true;
        [SerializeField] protected bool canMinimize = true;
        [SerializeField] protected bool canMaximize = true;
        [SerializeField] protected bool canClose = true;

        [Header("References")]
        [SerializeField] protected RectTransform titleBar;
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button minimizeButton;
        [SerializeField] protected Button maximizeButton;
        [SerializeField] protected GameObject contentPanel;

        [Inject(required: false)] protected IWindowManager _windowManager;
        [Inject(required: false)] protected IEventService _eventService;
        [Inject(required: false)] protected IAudioService _audioService;

        protected RectTransform _rectTransform;
        protected CanvasGroup _canvasGroup;
        protected Canvas _canvas;

        protected WindowDragHandler _dragHandler;
        protected WindowResizeHandler _resizeHandler;

        protected bool _isOpen;
        protected bool _isMinimized;
        protected bool _isMaximized;
        protected bool _isFocused;
        protected int _zOrder;

        protected Vector2 _normalizedPosition;
        protected Vector2 _normalizedSize;

        // IWindow Properties
        public string WindowId => windowId;
        public string WindowTitle => windowTitle;
        public bool IsOpen => _isOpen;
        public bool IsMinimized => _isMinimized;
        public bool IsMaximized => _isMaximized;
        public bool IsFocused => _isFocused;
        public int ZOrder
        {
            get => _zOrder;
            set
            {
                _zOrder = value;
                UpdateCanvasSortingOrder(); // FIX: Canvas sorting order güncelleniyor
            }
        }
        public RectTransform RectTransform => _rectTransform;

        protected virtual void Awake()
        {
            InitializeComponents();
            ValidateWindowId();
            SetupCanvas();
            SetupHandlers();
            SetupButtons();
            //gameObject.SetActive(false);
        }

        protected virtual void Start()
        {
            InjectDependencies();

            if (!_isOpen)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_rectTransform == null)
            {
                Debug.LogError($"[WindowBase] RectTransform not found on {gameObject.name}");
            }

            if (_canvasGroup == null)
            {
                Debug.LogError($"[WindowBase] CanvasGroup not found on {gameObject.name}");
            }
        }

        /// <summary>
        /// Ensure windowId is set
        /// </summary>
        private void ValidateWindowId()
        {
            if (string.IsNullOrEmpty(windowId))
            {
                windowId = GetType().Name;
                Debug.LogWarning($"[WindowBase] WindowId was empty, set to {windowId}");
            }
        }

        /// <summary>
        /// Setup canvas for proper rendering
        /// </summary>
        private void SetupCanvas()
        {
            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }

            _canvas.overrideSorting = true;
            UpdateCanvasSortingOrder(); // FIX: Initial sorting order set ediliyor

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        /// <summary>
        /// FIX: Canvas sorting order'ı güncelle
        /// </summary>
        private void UpdateCanvasSortingOrder()
        {
            if (_canvas != null)
            {
                _canvas.sortingOrder = _zOrder;
            }
        }

        /// <summary>
        /// Setup drag and resize handlers
        /// Single Responsibility: Separated into dedicated components
        /// </summary>
        private void SetupHandlers()
        {
            if (titleBar != null)
            {
                _dragHandler = gameObject.GetComponent<WindowDragHandler>();
                if (_dragHandler == null)
                {
                    _dragHandler = gameObject.AddComponent<WindowDragHandler>();
                }
                _dragHandler.Initialize(this, titleBar);
            }

            if (canResize)
            {
                _resizeHandler = gameObject.GetComponent<WindowResizeHandler>();
                if (_resizeHandler == null)
                {
                    _resizeHandler = gameObject.AddComponent<WindowResizeHandler>();
                }
                _resizeHandler.Initialize(this, minSize, maxSize);
            }
        }

        /// <summary>
        /// Setup button listeners
        /// </summary>
        protected virtual void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
                closeButton.gameObject.SetActive(canClose);
            }

            if (minimizeButton != null)
            {
                minimizeButton.onClick.RemoveAllListeners();
                minimizeButton.onClick.AddListener(Minimize);
                minimizeButton.gameObject.SetActive(canMinimize);
            }

            if (maximizeButton != null)
            {
                maximizeButton.onClick.RemoveAllListeners();
                maximizeButton.onClick.AddListener(ToggleMaximize);
                maximizeButton.gameObject.SetActive(canMaximize);
            }
        }

        /// <summary>
        /// Inject dependencies
        /// </summary>
        private void InjectDependencies()
        {
            Dependencies.Inject(this);

            if (_eventService == null)
            {
                Debug.LogWarning($"[{windowId}] EventService not available - events will not be published");
            }

            if (_audioService == null)
            {
                Debug.LogWarning($"[{windowId}] AudioService not available - no sound effects");
            }
        }

        // ==================== IWindow Implementation ====================

        public virtual void Open()
        {
            if (_isOpen)
            {
                Debug.LogWarning($"[{windowId}] Window is already open");
                return;
            }

            gameObject.SetActive(true);
            _isOpen = true;
            _isFocused = true;

            _audioService?.PlayUISound(UISoundType.Open);
            _eventService?.Publish(new WindowOpenedEvent { WindowId = windowId, ZOrder = _zOrder });

            OnOpen();

            Debug.Log($"[{windowId}] Window opened");
        }

        public virtual void Close()
        {
            if (!_isOpen)
            {
                Debug.LogWarning($"[{windowId}] Window is not open");
                return;
            }

            if (!canClose)
            {
                Debug.LogWarning($"[{windowId}] Window cannot be closed");
                return;
            }

            _isOpen = false;
            _isFocused = false;

            _audioService?.PlayUISound(UISoundType.Close);
            _eventService?.Publish(new WindowClosedEvent { WindowId = windowId });

            OnClose();

            gameObject.SetActive(false);

            Debug.Log($"[{windowId}] Window closed");
        }

        public virtual void Minimize()
        {
            if (!_isOpen || _isMinimized || !canMinimize)
            {
                Debug.LogWarning($"[{windowId}] Cannot minimize (Open:{_isOpen}, Minimized:{_isMinimized}, CanMinimize:{canMinimize})");
                return;
            }

            _isMinimized = true;
            if (contentPanel != null)
            {
                contentPanel.SetActive(false);
            }

            _audioService?.PlayUISound(UISoundType.Click);
            _eventService?.Publish(new WindowMinimizedEvent { WindowId = windowId });

            OnMinimize();

            Debug.Log($"[{windowId}] Window minimized");
        }

        public virtual void Maximize()
        {
            if (!_isOpen || _isMaximized || !canMaximize)
            {
                Debug.LogWarning($"[{windowId}] Cannot maximize (Open:{_isOpen}, Maximized:{_isMaximized}, CanMaximize:{canMaximize})");
                return;
            }

            _normalizedPosition = _rectTransform.anchoredPosition;
            _normalizedSize = _rectTransform.sizeDelta;

            RectTransform parentRect = _rectTransform.parent as RectTransform;
            if (parentRect != null)
            {
                _rectTransform.anchoredPosition = Vector2.zero;
                _rectTransform.sizeDelta = parentRect.rect.size;
            }

            _isMaximized = true;

            _audioService?.PlayUISound(UISoundType.Click);
            _eventService?.Publish(new WindowMaximizedEvent { WindowId = windowId });

            OnMaximize();

            Debug.Log($"[{windowId}] Window maximized");
        }

        public virtual void Restore()
        {
            if (_isMinimized)
            {
                _isMinimized = false;
                if (contentPanel != null)
                {
                    contentPanel.SetActive(true);
                }

                _eventService?.Publish(new WindowRestoredEvent { WindowId = windowId });
                OnRestore();

                Debug.Log($"[{windowId}] Window restored from minimized");
            }
            else if (_isMaximized)
            {
                _isMaximized = false;
                _rectTransform.anchoredPosition = _normalizedPosition;
                _rectTransform.sizeDelta = _normalizedSize;

                _eventService?.Publish(new WindowRestoredEvent { WindowId = windowId });
                OnRestore();

                Debug.Log($"[{windowId}] Window restored from maximized");
            }

            _audioService?.PlayUISound(UISoundType.Click);
        }

        protected virtual void ToggleMaximize()
        {
            if (_isMaximized)
            {
                Restore();
            }
            else
            {
                Maximize();
            }
        }

        public virtual void Focus()
        {
            if (!_isOpen)
            {
                Debug.LogWarning($"[{windowId}] Cannot focus - window is not open");
                return;
            }

            if (_isFocused) return;

            _isFocused = true;
            _eventService?.Publish(new WindowFocusedEvent { WindowId = windowId });

            OnFocus();
        }

        public virtual void Blur()
        {
            if (!_isFocused) return;

            _isFocused = false;
            OnBlur();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _windowManager?.BringToFront(this);
        }

        // ==================== Virtual Methods for Subclasses ====================

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnMinimize() { }
        protected virtual void OnMaximize() { }
        protected virtual void OnRestore() { }
        protected virtual void OnFocus() { }
        protected virtual void OnBlur() { }

        // ==================== Cleanup ====================

        protected virtual void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (minimizeButton != null) minimizeButton.onClick.RemoveListener(Minimize);
            if (maximizeButton != null) maximizeButton.onClick.RemoveListener(ToggleMaximize);

            Debug.Log($"[{windowId}] Window destroyed");
        }
    }

    // =============== SEPARATION OF CONCERNS ===============

    /// <summary>
    /// Handles window dragging functionality
    /// Single Responsibility Principle: Only manages drag operations
    /// </summary>
    public class WindowDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private WindowBase _window;
        private RectTransform _titleBar;
        private Vector2 _dragOffset;
        private bool _isDragging;
        private IEventService _eventService;

        public void Initialize(WindowBase window, RectTransform titleBar)
        {
            _window = window;
            _titleBar = titleBar;

            if (Dependencies.Container.TryResolve(out IEventService eventService))
            {
                _eventService = eventService;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_window == null || _titleBar == null) return;
            if (_window.IsMaximized) return;

            if (RectTransformUtility.RectangleContainsScreenPoint(_titleBar, eventData.position, eventData.pressEventCamera))
            {
                _isDragging = true;

                RectTransform parent = _window.RectTransform.parent as RectTransform;
                if (parent != null)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parent,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPoint);

                    _dragOffset = _window.RectTransform.anchoredPosition - localPoint;
                }

                _eventService?.Publish(new WindowDragStartedEvent { WindowId = _window.WindowId });
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _window == null || _window.IsMaximized) return;

            RectTransform parent = _window.RectTransform.parent as RectTransform;
            if (parent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint);

                _window.RectTransform.anchoredPosition = localPoint + _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _eventService?.Publish(new WindowDragEndedEvent { WindowId = _window.WindowId });
            }
        }
    }

    /// <summary>
    /// Handles window resize functionality
    /// Single Responsibility Principle: Only manages resize operations
    /// </summary>
    public class WindowResizeHandler : MonoBehaviour
    {
        private WindowBase _window;
        private Vector2 _minSize;
        private Vector2 _maxSize;

        public void Initialize(WindowBase window, Vector2 minSize, Vector2 maxSize)
        {
            _window = window;
            _minSize = minSize;
            _maxSize = maxSize;
        }

        public void SetMinSize(Vector2 size)
        {
            _minSize = size;
            ClampWindowSize();
        }

        public void SetMaxSize(Vector2 size)
        {
            _maxSize = size;
            ClampWindowSize();
        }

        private void ClampWindowSize()
        {
            if (_window == null || _window.RectTransform == null) return;

            Vector2 currentSize = _window.RectTransform.sizeDelta;
            Vector2 clampedSize = new Vector2(
                Mathf.Clamp(currentSize.x, _minSize.x, _maxSize.x),
                Mathf.Clamp(currentSize.y, _minSize.y, _maxSize.y)
            );

            if (currentSize != clampedSize)
            {
                _window.RectTransform.sizeDelta = clampedSize;
            }
        }
    }
}