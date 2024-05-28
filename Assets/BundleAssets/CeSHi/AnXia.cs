using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnXia : MonoBehaviour
{
    // Start is called before the first frame update
    public Button buton;
    void Start()
    {
        buton.onClick.AddListener(() =>
        {
            GameManager.UI.OpenUI(UIViewID.LoginUI);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
