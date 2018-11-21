using UnityEngine;
using System.Collections;


[RequireComponent(typeof(MMT.MobileMovieTexture))]
public class TestMobileTexture : MonoBehaviour 
{
    private MMT.MobileMovieTexture m_movieTexture;

    void Awake()
    {
        m_movieTexture = GetComponent<MMT.MobileMovieTexture>();

        m_movieTexture.onFinished += OnFinished;
    }

    void OnFinished(MMT.MobileMovieTexture sender)
    {
        Debug.Log(sender.Path + " has finished ");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0.0f, 0.0f, Screen.width, Screen.height));

        var currentPosition = (float)m_movieTexture.PlayPosition;
		
		var newPosition = GUILayout.HorizontalSlider(currentPosition,0.0f,(float)m_movieTexture.Duration);

        if (newPosition != currentPosition)
        {
			m_movieTexture.PlayPosition = newPosition;
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();

		if (GUILayout.Button(m_movieTexture.IsPlaying ? "Pause" : "Play"))
		{
			if(m_movieTexture.IsPlaying)
			{
				m_movieTexture.Pause = true;
			}
			else 
			{
				if(!m_movieTexture.Pause)
				{
					m_movieTexture.Play();
				}
				else
				{
					m_movieTexture.Pause = false;
				}
			}

		}
		
		if (GUILayout.Button("Stop"))
		{
			m_movieTexture.Stop();
		}

        GUILayout.EndHorizontal();

        GUILayout.EndArea();

     }
}
