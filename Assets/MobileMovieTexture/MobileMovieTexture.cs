using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace MMT
{
    public class MobileMovieTexture : MonoBehaviour
    {
        #region Types

        public delegate void OnFinished(MobileMovieTexture sender);

        #endregion

        #region Editor Variables

        /// <summary>
        /// File path to the video file, includes the extension, usually .ogg or .ogv
        /// </summary>
#if UNITY_EDITOR
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
		[StreamingAssetsLinkAttribute(typeof(MovieTexture), "Movie")]
#else
        [StreamingAssetsLinkAttribute(typeof(UnityEngine.Object), "Movie")]
#endif
#endif
		[SerializeField]
        private string m_path;

        /// <summary>
        /// Material(s) to decode the movie on to, MMT sets up the textures and the texture scale/offset
        /// </summary>
        [SerializeField]
        private Material[] m_movieMaterials;

        /// <summary>
        /// Whether to start playing automatically, be careful to set advance
        /// </summary>
        [SerializeField]
        private bool m_playAutomatically = true;

        /// <summary>
        /// Whether the movie should advance, used to pause, or also to just decode the first frame
        /// </summary>
        [SerializeField]
        private bool m_advance = true;

        /// <summary>
        /// How many times to loop, -1 == infinite
        /// </summary>
        [SerializeField]
        private int m_loopCount = -1;

        /// <summary>
        /// Playback speed, has to be positive, can't play backwards
        /// </summary>
        [SerializeField]
        private float m_playSpeed = 1.0f;

        /// <summary>
        /// Whether to scan the duration of the movie on opening. This makes opening movies more expensive as it reads the whole file. Ideally cache this off if you need it
        /// </summary>
        [SerializeField]
        private bool m_scanDuration = true;

        /// <summary>
        /// When seeking, it tries to find a keyframe to seek to, however it often fails. If this is set, after a seek, it will decode till it hits a keyframe. Without it set, you may see artifacts on a seek
        /// </summary>
        [SerializeField]
        private bool m_seekKeyFrame = false;

        #endregion

        #region Other Variables

        private IntPtr m_nativeContext = IntPtr.Zero;
        private IntPtr m_nativeTextureContext = IntPtr.Zero;

        private int m_picX = 0;
        private int m_picY = 0;

        private int m_yStride = 0;
        private int m_yHeight = 0;
        private int m_uvStride = 0;
        private int m_uvHeight = 0;

        private Vector2 m_uvYScale;
        private Vector2 m_uvYOffset;
        
        private Vector2 m_uvCrCbScale;
        private Vector2 m_uvCrCbOffset;

        private const int CHANNELS = 3; //Y,Cb,Cr
        private Texture2D[] m_ChannelTextures = new Texture2D[CHANNELS];

        private double m_elapsedTime;

        private bool m_hasFinished = true;

        public MobileMovieTexture()
        {
            Height = 0;
            Width = 0;
        }

        #endregion

        /// <summary>
        /// Function to call on finish
        /// </summary>
        public event OnFinished onFinished;

        #region Properties

        /// <summary>
        /// File path to the video file, includes the extension, usually .ogg or .ogv
        /// </summary>
        public string Path { get { return m_path; } set { m_path = value; } }

        /// <summary>
        /// Whether the path is absolute or in the streaming assets directory
        /// </summary>
		public bool AbsolutePath { get; set; }

        /// <summary>
        /// Material to decode the movie on to, MMT sets up the textures and the texture scale/offset
        /// </summary>
        public Material[] MovieMaterial { get { return m_movieMaterials; } }

        /// <summary>
        /// Whether to start playing automatically, be careful to set advance
        /// </summary>
        public bool PlayAutomatically { set { m_playAutomatically = value; } }

        /// <summary>
        /// How many times to loop, -1 == infinite
        /// </summary>
        public int LoopCount { get { return m_loopCount; } set { m_loopCount = value; } }

        /// <summary>
        /// Playback speed, has to be positive, can't play backwards
        /// </summary>
        public float PlaySpeed { get { return m_playSpeed; } set { m_playSpeed = value; } }

        /// <summary>
        /// Whether to scan the duration of the movie on opening. This makes opening movies more expensive as it reads the whole file. Ideally cache this off if you need it
        /// </summary>
        public bool ScanDuration { get { return m_scanDuration; } set { m_scanDuration = value; } }

        /// <summary>
        /// When seeking, it tries to find a keyframe to seek to, however it often fails. If this is set, after a seek, it will decode till it hits a keyframe. Without it set, you may see artifacts on a seek
        /// </summary>
        public bool SeekKeyFrame { get { return m_seekKeyFrame; } set { m_seekKeyFrame = value; } }

        /// <summary>
        /// Width of the movie in pixels
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the movie in pixels
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Aspect ratio (width/height) of movie
        /// </summary>
        public float AspectRatio
        {
            get
            {
                if (m_nativeContext != IntPtr.Zero)
                {
                    return GetAspectRatio(m_nativeContext);
                }
                else
                {
                    return 1.0f;
                }
            }
        }

        /// <summary>
        /// Frames per second of movie
        /// </summary>
        public double FPS
        {
            get
            {
                if (m_nativeContext != IntPtr.Zero)
                {
                    return GetVideoFPS(m_nativeContext);
                }
                else
                {
                    return 1.0;
                }
            }
        }

        /// <summary>
        /// Is the movie currently playing
        /// </summary>
        public bool IsPlaying
        {
            get { return m_nativeContext != IntPtr.Zero && !m_hasFinished && m_advance; }
        }

        public bool Pause { get { return !m_advance; } set { m_advance = !value; } }

        /// <summary>
        /// Use this to retrieve the play position and to seek. NB after you seek, the play position will not be exactly what you seeked to, as it is tries to find a key frame
        /// </summary>
        public double PlayPosition
        {
            get { return m_elapsedTime; }
            set 
            {
                if (m_nativeContext != IntPtr.Zero)
                {
                    m_elapsedTime = Seek(m_nativeContext, value, m_seekKeyFrame);
                }
            }
        }

        /// <summary>
        /// The length of the movie, this is only valid if you have ScanDuration set
        /// </summary>
        public double Duration
        {
            get { return m_nativeContext != IntPtr.Zero ? GetDuration(m_nativeContext) : 0.0; }
        }

        #endregion

        #region Native Interface

#if UNITY_IPHONE && !UNITY_EDITOR
    private const string PLATFORM_DLL = "__Internal";
#else
        private const string PLATFORM_DLL = "theorawrapper";
#endif
        [DllImport(PLATFORM_DLL)]
        private static extern IntPtr CreateContext();

        [DllImport(PLATFORM_DLL)]
        private static extern void DestroyContext(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern bool OpenStream(IntPtr context, string path, int offset, int size, bool pot, bool scanDuration, int maxSkipFrames);

        [DllImport(PLATFORM_DLL)]
        private static extern void CloseStream(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetPicWidth(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetPicHeight(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetPicX(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetPicY(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetYStride(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetYHeight(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetUVStride(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern int GetUVHeight(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern bool HasFinished(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern double GetDecodedFrameTime(IntPtr context);

		[DllImport(PLATFORM_DLL)]
		private static extern double GetUploadedFrameTime(IntPtr context);

		[DllImport(PLATFORM_DLL)]
		private static extern double GetTargetDecodeFrameTime(IntPtr context);
        
        [DllImport(PLATFORM_DLL)]
        private static extern void SetTargetDisplayDecodeTime(IntPtr context, double targetTime);

        [DllImport(PLATFORM_DLL)]
        private static extern double GetVideoFPS(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern float GetAspectRatio(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern double Seek(IntPtr context, double seconds, bool waitKeyFrame);

        [DllImport(PLATFORM_DLL)]
        private static extern double GetDuration(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern IntPtr GetNativeHandle(IntPtr context, int planeIndex);

        [DllImport(PLATFORM_DLL)]
        private static extern IntPtr GetNativeTextureContext(IntPtr context);

        [DllImport(PLATFORM_DLL)]
        private static extern void SetPostProcessingLevel(IntPtr context, int level);

        #endregion

        #region Behaviour Overrides

        void Start()
        {
            m_nativeContext = CreateContext();

            if (m_nativeContext == IntPtr.Zero)
            {
                //Debug.LogError("Unable to create Mobile Movie Texture native context");
                return;
            }

            if (m_playAutomatically)
            {
                Play();
            }
        }

        void OnDestroy()
        {
            DestroyTextures();
            DestroyContext(m_nativeContext);
        }

        void Update()
        {
            if (m_nativeContext != IntPtr.Zero && !m_hasFinished)
            {
                //Texture context can change when resizing windows
                //when put into the background etc
                var textureContext = GetNativeTextureContext(m_nativeContext);

                if (textureContext != m_nativeTextureContext)
                {
                    DestroyTextures();
                    AllocateTexures();

                    m_nativeTextureContext = textureContext;
                }

                m_hasFinished = HasFinished(m_nativeContext);

                if (!m_hasFinished)
                {
                    if (m_advance)
                    {
                        m_elapsedTime += Time.deltaTime * Mathf.Max(m_playSpeed, 0.0f);
                    }
                }
                else
                {
                    if ((m_loopCount - 1) > 0 || m_loopCount == -1)
                    {
                        if (m_loopCount != -1)
                        {
                            m_loopCount--;
                        }

                        m_elapsedTime = m_elapsedTime % GetDecodedFrameTime(m_nativeContext);

                        Seek(m_nativeContext, 0, false);

                        m_hasFinished = false;
                    }
                    else if (onFinished != null)
                    {
						m_elapsedTime = GetDecodedFrameTime(m_nativeContext);

                        onFinished(this);
                    }

                }

                SetTargetDisplayDecodeTime(m_nativeContext, m_elapsedTime);

            }
        }


        #endregion

        #region Methods

        public void Play()
        {
            m_elapsedTime = 0.0;

            Open();

            m_hasFinished = false;

            //Create a manager if we don't have one
            if (MobileMovieManager.Instance == null)
            {
                gameObject.AddComponent<MobileMovieManager>();
            }
        }

        public void Stop()
        {
            CloseStream(m_nativeContext);
			m_hasFinished = true;
        }

        private void Open()
        {
            string path = m_path;
            long offset = 0;
            long length = 0;

            if (!AbsolutePath)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        path = Application.dataPath;

                        if (!AssetStream.GetZipFileOffsetLength(Application.dataPath, m_path, out offset, out length))
                        {
                            return;
                        }
                        break;
                    default:
                        path = Application.streamingAssetsPath + "/" + m_path;
                        break;
                }
            }


            //No platform should need power of 2 textures anymore
            const bool powerOf2Textures = false;

            //This is maximum frames decoded before a frame is uploaded
            const int maxSkipFrames = 16;

            if (m_nativeContext != IntPtr.Zero && OpenStream(m_nativeContext, path, (int)offset, (int)length, powerOf2Textures, m_scanDuration, maxSkipFrames))
            {
                Width = GetPicWidth(m_nativeContext);
                Height = GetPicHeight(m_nativeContext);

                m_picX = GetPicX(m_nativeContext);
                m_picY = GetPicY(m_nativeContext);

				m_yStride = GetYStride(m_nativeContext);
				m_yHeight = GetYHeight(m_nativeContext);
				m_uvStride = GetUVStride(m_nativeContext);
				m_uvHeight = GetUVHeight(m_nativeContext);

                CalculateUVScaleOffset();
            }
            else
            {
                //Debug.LogError("Unable to open movie " + m_nativeContext, this);
            }
        }

        private void AllocateTexures()
        {
            m_ChannelTextures[0] = Texture2D.CreateExternalTexture(m_yStride, m_yHeight, TextureFormat.Alpha8, false, false, GetNativeHandle(m_nativeContext, 0));
            m_ChannelTextures[1] = Texture2D.CreateExternalTexture(m_uvStride, m_uvHeight, TextureFormat.Alpha8, false, false, GetNativeHandle(m_nativeContext, 1));
            m_ChannelTextures[2] = Texture2D.CreateExternalTexture(m_uvStride, m_uvHeight, TextureFormat.Alpha8, false, false, GetNativeHandle(m_nativeContext, 2));
            
            if (m_movieMaterials != null)
            {
                for (int i = 0; i < m_movieMaterials.Length; ++i)
                {
                    var mat = m_movieMaterials[i];

                    if (mat != null)
                    {
                        SetTextures(mat);
                    }
                }
            }
        }

        public void SetTextures(Material material)
        {
            material.SetTexture("_YTex", m_ChannelTextures[0]);
            material.SetTexture("_CbTex", m_ChannelTextures[1]);
			material.SetTexture("_CrTex", m_ChannelTextures[2]);

            material.SetTextureScale("_YTex", m_uvYScale);
            material.SetTextureOffset("_YTex", m_uvYOffset);

            material.SetTextureScale("_CbTex", m_uvCrCbScale);
            material.SetTextureOffset("_CbTex", m_uvCrCbOffset);
        }

        public void RemoveTextures(Material material)
        {
            material.SetTexture("_YTex", null);
			material.SetTexture("_CbTex", null);
            material.SetTexture("_CrTex", null);
        }

        private void CalculateUVScaleOffset()
        {
			var picWidth = (float)Width;
			var picHeight = (float)Height;
			var picX = (float)m_picX;
			var picY = (float)m_picY;
			var yStride = (float)m_yStride;
			var yHeight = (float)m_yHeight;
			var uvStride = (float)m_uvStride;
			var uvHeight = (float)m_uvHeight;



            m_uvYScale = new Vector2(picWidth / yStride, -(picHeight / yHeight));
            m_uvYOffset = new Vector2(picX / yStride, (picHeight + picY) / yHeight);

            m_uvCrCbScale = new Vector2();
            m_uvCrCbOffset = new Vector2();

            if (m_uvStride == m_yStride)
            {
                m_uvCrCbScale.x = m_uvYScale.x;
            }
            else
            {
                m_uvCrCbScale.x = (picWidth / 2.0f) / uvStride;
            }

            if (m_uvHeight == m_yHeight)
            {
                m_uvCrCbScale.y = m_uvYScale.y;
                m_uvCrCbOffset = m_uvYOffset;
            }
            else
            {
                m_uvCrCbScale.y = -((picHeight / 2.0f) / uvHeight);
                m_uvCrCbOffset = new Vector2((picX / 2.0f) / uvStride, (((picHeight + picY) / 2.0f) / uvHeight));
            }
        }

        private void DestroyTextures()
        {
            if (m_movieMaterials != null)
            {
                for (int i = 0; i < m_movieMaterials.Length; ++i)
                {
                    var mat = m_movieMaterials[i];

                    if (mat != null)
                    {
                        RemoveTextures(mat);
                    }
                }
            }

            for (int i = 0; i < CHANNELS; ++i)
            {
                if (m_ChannelTextures[i] != null)
                {
                    Destroy(m_ChannelTextures[i]);
                    m_ChannelTextures[i] = null;
                }
            }
        }

       
        #endregion
    }
}

