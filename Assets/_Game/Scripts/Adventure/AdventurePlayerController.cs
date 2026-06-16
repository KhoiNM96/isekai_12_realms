using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Isekai12Realms.Adventure
{
    public class AdventurePlayerController : MonoBehaviour
    {
        [SerializeField] private RectTransform playerRectTransform;
        [SerializeField] private RectTransform worldBounds;
        [SerializeField] private RectTransform viewportBounds;
        [SerializeField] private float moveSpeed = 420f;
        [SerializeField] private float jumpVelocity = 920f;
        [SerializeField] private float gravity = 2400f;
        [SerializeField] private float tapMoveThreshold = 20f;

        private readonly List<PlatformSegmentData> platforms = new List<PlatformSegmentData>();
        private float horizontalInput;
        private bool jumpRequested;
        private bool dropRequested;
        private bool movementEnabled = true;
        private float verticalVelocity;
        private bool grounded;
        private bool ignoreOneWayPlatforms;
        private float ignoreOneWayTimer;

        public RectTransform PlayerRectTransform => playerRectTransform != null ? playerRectTransform : GetComponent<RectTransform>();
        public bool IsGrounded => grounded;

        public void Initialize(RectTransform bounds)
        {
            Initialize(bounds, null);
        }

        public void Initialize(RectTransform worldRoot, RectTransform viewportRoot)
        {
            worldBounds = worldRoot;
            viewportBounds = viewportRoot;
            playerRectTransform = GetComponent<RectTransform>();
        }

        public void SetPlatforms(List<PlatformSegmentData> segments)
        {
            platforms.Clear();
            if (segments != null)
            {
                platforms.AddRange(segments);
            }
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
            {
                horizontalInput = 0f;
                jumpRequested = false;
                dropRequested = false;
            }
        }

        public void SetMoveInput(float input)
        {
            horizontalInput = Mathf.Clamp(input, -1f, 1f);
        }

        public void RequestJump()
        {
            jumpRequested = true;
        }

        public void RequestDropThrough()
        {
            dropRequested = true;
        }

        public void SetSpawnPosition(Vector2 position)
        {
            if (PlayerRectTransform == null)
            {
                return;
            }

            PlayerRectTransform.anchoredPosition = Clamp(position);
            verticalVelocity = 0f;
            grounded = false;
            UpdateCamera(PlayerRectTransform.anchoredPosition.x);
        }

        public void MoveBy(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > 0.01f)
            {
                SetMoveInput(Mathf.Sign(delta.x));
            }

            if (delta.y > 0f)
            {
                RequestJump();
            }
        }

        public Vector2 GetPosition()
        {
            return PlayerRectTransform != null ? PlayerRectTransform.anchoredPosition : Vector2.zero;
        }

        private void Update()
        {
            if (PlayerRectTransform == null)
            {
                return;
            }

            HandleInput();
            if (!movementEnabled)
            {
                return;
            }

            if (ignoreOneWayTimer > 0f)
            {
                ignoreOneWayTimer -= Time.deltaTime;
                if (ignoreOneWayTimer <= 0f)
                {
                    ignoreOneWayPlatforms = false;
                }
            }

            Vector2 current = PlayerRectTransform.anchoredPosition;
            if (jumpRequested && grounded)
            {
                verticalVelocity = jumpVelocity;
                grounded = false;
                jumpRequested = false;
            }

            if (dropRequested)
            {
                ignoreOneWayPlatforms = true;
                ignoreOneWayTimer = 0.3f;
                dropRequested = false;
            }

            verticalVelocity -= gravity * Time.deltaTime;
            Vector2 next = current + new Vector2(horizontalInput * moveSpeed * Time.deltaTime, verticalVelocity * Time.deltaTime);
            next = Clamp(next);
            ResolvePlatformCollision(ref next, current);
            PlayerRectTransform.anchoredPosition = next;
            UpdateCamera(next.x);
        }

        private void HandleInput()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                float keyboardInput = 0f;
                if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed) keyboardInput -= 1f;
                if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed) keyboardInput += 1f;
                if (Mathf.Abs(keyboardInput) > 0.01f) horizontalInput = keyboardInput;
                if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame) RequestJump();
                if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed || UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed) dropRequested = true;
            }

            if (UnityEngine.InputSystem.Pointer.current != null && UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame)
            {
                TrySetTarget(UnityEngine.InputSystem.Pointer.current.position.ReadValue());
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            float keyboardInput = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) keyboardInput -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) keyboardInput += 1f;
            if (Mathf.Abs(keyboardInput) > 0.01f) horizontalInput = keyboardInput;
            if (Input.GetKeyDown(KeyCode.Space)) RequestJump();
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dropRequested = true;
            if (Input.GetMouseButtonDown(0))
            {
                TrySetTarget(Input.mousePosition);
            }
#endif
        }

        private void TrySetTarget(Vector2 screenPosition)
        {
            RectTransform targetBounds = viewportBounds != null ? viewportBounds : worldBounds;
            if (targetBounds == null || PlayerRectTransform == null)
            {
                return;
            }

            Camera camera = Camera.main;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(targetBounds, screenPosition, camera, out Vector2 localPoint))
            {
                return;
            }

            if (localPoint.magnitude < tapMoveThreshold)
            {
                return;
            }

            Vector2 current = PlayerRectTransform.anchoredPosition;
            horizontalInput = Mathf.Clamp((localPoint.x - current.x) / 300f, -1f, 1f);
            if (localPoint.y > current.y + 90f)
            {
                RequestJump();
            }
        }

        private Vector2 Clamp(Vector2 position)
        {
            if (worldBounds == null)
            {
                return position;
            }

            Vector2 size = worldBounds.rect.size;
            float halfWidth = PlayerRectTransform != null ? PlayerRectTransform.rect.width * 0.5f : 48f;
            float x = Mathf.Clamp(position.x, halfWidth, Mathf.Max(halfWidth, size.x - halfWidth));
            float y = Mathf.Clamp(position.y, -size.y * 0.5f + 48f, size.y * 0.5f - 48f);
            return new Vector2(x, y);
        }

        private void UpdateCamera(float playerX)
        {
            if (worldBounds == null || viewportBounds == null)
            {
                return;
            }

            float viewportWidth = viewportBounds.rect.width;
            float worldWidth = worldBounds.rect.width;
            float targetX = Mathf.Clamp((viewportWidth * 0.5f) - playerX, viewportWidth - worldWidth, 0f);
            worldBounds.anchoredPosition = new Vector2(targetX, worldBounds.anchoredPosition.y);
        }

        private void ResolvePlatformCollision(ref Vector2 next, Vector2 previous)
        {
            Vector2 halfSize = PlayerRectTransform != null ? PlayerRectTransform.rect.size * 0.5f : new Vector2(48f, 48f);
            grounded = false;

            if (platforms.Count == 0)
            {
                if (next.y <= -halfSize.y)
                {
                    next.y = -halfSize.y;
                    verticalVelocity = 0f;
                    grounded = true;
                }

                return;
            }

            for (int i = 0; i < platforms.Count; i++)
            {
                PlatformSegmentData platform = platforms[i];
                if (platform == null)
                {
                    continue;
                }

                Rect rect = new Rect(platform.position, platform.size);
                float playerLeft = next.x - halfSize.x;
                float playerRight = next.x + halfSize.x;
                float playerBottom = next.y - halfSize.y;
                float previousBottom = previous.y - halfSize.y;
                float platformTop = rect.yMax;
                bool horizontalOverlap = playerRight > rect.xMin && playerLeft < rect.xMax;

                if (!horizontalOverlap)
                {
                    continue;
                }

                bool falling = verticalVelocity <= 0f;
                bool crossedTop = previousBottom >= platformTop && playerBottom <= platformTop + 4f;
                if (falling && crossedTop && (!platform.oneWay || !ignoreOneWayPlatforms))
                {
                    next.y = platformTop + halfSize.y;
                    verticalVelocity = 0f;
                    grounded = true;
                }
            }

            if (!grounded && next.y <= -halfSize.y)
            {
                next.y = -halfSize.y;
                verticalVelocity = 0f;
                grounded = true;
            }
        }
    }
}
