using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField]
    private bool autoInitialize;

    [SerializeField]
    private View[] views;

    [SerializeField]
    private View defaultView;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (autoInitialize)
        {
            Initialize();
        }
    }
    public void Initialize()
    {
        foreach(View view in views)
        {
            view.Initialize();

            view.Hide();
        }

        if(defaultView != null)
        {
            defaultView.Show();
        }  
    }

    public void Show<T>(object args = null) where T : View
    {
        foreach(View view in views)
        {
            if(view is T)
            {
                view.Show(args);
            }
            else
            {
                view.Hide();
            }
            //view.gameObject.SetActive(view is T);
        }
    }
}
