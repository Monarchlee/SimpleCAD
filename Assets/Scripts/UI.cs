using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI : MonoBehaviour
{
    bool isDisplay = false;
    public bool isSave=true;
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
            Application.Quit();
        else;
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

    }
    public void SaveClick()
    {
        isSave = true;
    }
    public void SaveAsClick()
    {

    }
    public void SizeChange()
    {
        Slider slider = rootBrush.transform.Find("BrushSize").GetComponent<Slider>();
        Mouse.radius = slider.value;
    }
    public void StrengthChange()
    {
        Slider slider = rootBrush.transform.Find("BrushStrength").GetComponent<Slider>();
        Mouse.strength = slider.value;
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
    }
    public void ShapeChange()
    {
        Dropdown dropdown= rootBrush.transform.Find("Shape").GetComponent<Dropdown>();
        Mouse.shape = dropdown.value;
    }
}
