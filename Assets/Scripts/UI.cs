using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Windows.Forms;
using VolumeData;


public class UI : MonoBehaviour
{
    bool isDisplay = false;
    static public bool isSave=true;
    GameObject rootFile;
    GameObject rootBrush;
    // Start is called before the first frame update
    void Start()
    {
        rootFile = GameObject.Find("File");
        rootBrush = GameObject.Find("BrushSetting");
        GameObject.Find("New").SetActive(isDisplay);
        GameObject.Find("Open").SetActive(isDisplay);
        GameObject.Find("Save").SetActive(isDisplay);
        GameObject.Find("SaveAs").SetActive(isDisplay);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnGUI()
    {
        if (Event.current.Equals(Event.KeyboardEvent("^S")))
        {
            SaveClick();
        }
    }


    public void FileClick()
    {
        isDisplay = !isDisplay;
        rootFile.transform.Find("New").gameObject.SetActive(isDisplay);
        rootFile.transform.Find("Open").gameObject.SetActive(isDisplay);
        rootFile.transform.Find("Save").gameObject.SetActive(isDisplay);
        rootFile.transform.Find("SaveAs").gameObject.SetActive(isDisplay);
        
    }
    public void ExitClick()
    {
        if (isSave)
            UnityEngine.Application.Quit();
        else
            UnityEngine.Application.Quit();
            //todo;
    }
    public void BackWardClick()
    {
        //todo
    }
    public void ForwardClick()
    {
        //todo
    }
    public void NewClick()
    {

    }
    public void OpenClick()
    {
        OpenFileDialog dialog = new OpenFileDialog();
        Volume volume = null;
        //dialog.Filter = "exe files (*.exe)|*.exe"; 
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            volume = Volume.Deserialize(dialog.FileName);
        }
        Mouse mouse = GameObject.Find("Main Camera").GetComponent<Mouse>();
        mouse.target = volume;
        mouse.Redo();

    }
    public void SaveClick()
    {
        isSave = true;
    }
    public void SaveAsClick()
    {
        Mouse mouse = GameObject.Find("Main Camera").GetComponent<Mouse>();
        SaveFileDialog dialog = new SaveFileDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            Volume.Serialize(mouse.target, dialog.FileName);
            isSave = true;
        }

    }
    public void SizeChange()
    {
        Slider slider = rootBrush.transform.Find("BrushSize").GetComponent<Slider>();
        Mouse.radius = slider.value;
    }
    public void StrengthChange()
    {
        Slider slider = rootBrush.transform.Find("BrushStrength").GetComponent<Slider>();
        Mouse.strength = Mouse.iseraser? -slider.value:slider.value;
    }
    public void DampingChange()
    {
        Slider slider = rootBrush.transform.Find("BrushDamping").GetComponent<Slider>();
        Mouse.damping = slider.value;
    }
    public void EraserOn()
    {
        Toggle toggle = rootBrush.transform.Find("Eraser").GetComponent<Toggle>();
        Mouse.iseraser = toggle.isOn;
        Mouse.strength = -Mouse.strength;
    }
    public void MirrorXOn()
    {
        Toggle toggle = rootBrush.transform.Find("MirrorX").GetComponent<Toggle>();
        Mouse.mirror_x = toggle.isOn;
        Debug.Log(Mouse.mirror_x);
    }
    public void MirrorYOn()
    {
        Toggle toggle = rootBrush.transform.Find("MirrorY").GetComponent<Toggle>();
        Mouse.mirror_y = toggle.isOn;
    }
    public void MirrorZOn()
    {
        Toggle toggle = rootBrush.transform.Find("MirrorZ").GetComponent<Toggle>();
        Mouse.mirror_z = toggle.isOn;
    }
    public void ShapeChange()
    {
        Dropdown dropdown= rootBrush.transform.Find("Shape").GetComponent<Dropdown>();
        Mouse.shape = dropdown.value;
    }
}
