using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class MenuPanelUI : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        //StartCoroutine(CheckDayAndOpen());
    }
    
    private IEnumerator CheckDayAndOpen()
    {
        yield return null; // Aspetta un frame
        
        if (GameManager.instance != null && GameManager.instance.GetCurrentDay() >= 2)
        {
            OpenMenu();
        }
        else
        {
            NotifyOrderManagerToStart();
        }
    }

   
    public void OpenMenu()
    {
        
        gameObject.SetActive(true);
        Time.timeScale = 0;
    }


    public void CloseMenu()
    {
        Debug.Log("MenuPanelUI>> ### MenuPanel Open called");
        
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }
    
    private void NotifyOrderManagerToStart()
    {
        OrderManager orderManager = FindFirstObjectByType<OrderManager>();
        if (orderManager != null)
        {
            orderManager.StartOrderGeneration();
        }
    }
}