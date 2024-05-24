using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGame : MonoBehaviour
{
    // Start is called before the first frame update
    public Button button;
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            GameManager.Message.Post<MessageType.ButtonDown>(new MessageType.ButtonDown()).Coroutine();

        });

    }

    // Update is called once per frame
    void Update()
    {

    }
}
