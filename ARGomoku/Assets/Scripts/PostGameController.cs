using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

using MyGlobal;

public class PostGameController : MonoBehaviour
{
    private int userid;

    public string server_ip_address = "18.218.77.102";

    public TextMeshProUGUI Hint_Text_Box;
    public TextMeshProUGUI Title_Box;

   

    MockServer mock_server = new MockServer();

    Stage_Codes stage = Stage_Codes.checkwin;
    bool return_button_cicked = false;

    checkwin_json checkwin_response = new checkwin_json();
    bool checkwin_request_done = true;

    clearrecords_json clearrecords_response = new clearrecords_json();

    bool clearrecords_request_done = true;


    // Start is called before the first frame update
    void Start()
    {
        userid= GameObject.Find("userInfo").GetComponent<keepData>().userid;
        stage = Stage_Codes.checkwin;
        return_button_cicked = false;

        checkwin_request_done = true;
        clearrecords_request_done = true;
    }

    void Update(){
        switch (stage){
            case Stage_Codes.checkwin:
                checkwin_request_done = false;
                StartCoroutine(send_checkwin_request(userid));
                stage = Stage_Codes.checkwin_wait;
                break;
            case Stage_Codes.checkwin_wait:
                if (!checkwin_request_done){
                    // wait
                }
                else{
                    StopCoroutine(send_checkwin_request(userid));
                    string text_str = "";
                    string title_text ="You ";
                    if (checkwin_response.isWin){
                        title_text += "win";
                        text_str+="in "+ checkwin_response.num_piece.ToString()+" move.";
                    }
                    else{
                        title_text += "lose";
                        text_str+="in "+ checkwin_response.num_piece.ToString()+" move.";
                    }
                    modify_title_text(title_text);
                    modify_hint_text(text_str);
                    stage = Stage_Codes.do_nothing;
                }
                break;
            case Stage_Codes.do_nothing:
                if (return_button_cicked){
                    stage = Stage_Codes.clearrecords;
                }
                break;
            case Stage_Codes.clearrecords:
                clearrecords_request_done = false;
                StartCoroutine(send_clearrecords_request(userid));
                stage = Stage_Codes.clearrecords_wait;
                break;
            case Stage_Codes.clearrecords_wait:
                if (!clearrecords_request_done){
                    //wait
                }
                else{
                    StopCoroutine(send_clearrecords_request(userid));
                    return_button_cicked = false;
                    SceneManager.LoadScene("Scenes/StartGame");
                }
                
                break;

        }

    }


    enum Stage_Codes
    {
        do_nothing,
        checkwin,
        checkwin_wait,

        clearrecords,
        clearrecords_wait
    }


    public void return_button_onClick(){
        return_button_cicked = true;
        // http_request_handler.send_clearrecords_request(userid);
        // SceneManager.LoadScene("Scenes/StartGame");
    }

    private void modify_hint_text(string s, int fontsize = 48)
    {
        Hint_Text_Box.text = s;
        Hint_Text_Box.fontSize = fontsize;
    }
    private void modify_title_text(string s, int fontsize = 60)
    {
        Title_Box.text = s;
        Title_Box.fontSize = fontsize;
    }

    IEnumerator send_checkwin_request(int send_userid){
        // POST
        string uri = "https://18.218.77.102/checkwin/";
        // TODO: remove hard-defined rules
        WWWForm form = new WWWForm();
        form.AddField("userid", send_userid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                modify_hint_text("POST checkwin request error: " + webRequest.error);
                // checkwin_request_done = true;
            }
            else
            {
                // modify_hint_text("POST checkwin request success!");
                checkwin_response = JsonUtility.FromJson<checkwin_json>(webRequest.downloadHandler.text);
                checkwin_request_done = true; // Please put this line after putting the result into json class
            }
        }
    }

    IEnumerator send_clearrecords_request(int send_userid){
        // POST
        string uri = "https://18.218.77.102/clearrecords/";
        // TODO: remove hard-defined rules
        WWWForm form = new WWWForm();
        form.AddField("userid", send_userid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                modify_hint_text("POST clearrecords request error: " + webRequest.error);
                // clearrecords_request_done = true;
            }
            else
            {
                // modify_hint_text("POST clearrecords request success!");
                clearrecords_response = JsonUtility.FromJson<clearrecords_json>(webRequest.downloadHandler.text);
                clearrecords_request_done = true; // Please put this line after putting the result into json class
            }
        }
    }


    [Serializable]
    public class checkwin_json{
        public bool isWin;
        public int num_piece;
        public checkwin_json(){
            isWin = true;
            num_piece = 0;
        }
    }

   [Serializable]
    public class clearrecords_json{
        public string status;
        public clearrecords_json(){
            status = "done";
        }
    }

    public class MockServer{
        public checkwin_json check_stats_request(int send_userid){
            checkwin_json result = new checkwin_json();
            result.isWin = false;
            result.num_piece = 5;
            return result;
        }

        public clearrecords_json mock_clear_records(int send_userid){
            clearrecords_json result = new clearrecords_json();
            return result;
        }
    }


}
