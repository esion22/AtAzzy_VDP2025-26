using UnityEngine;

/*
 * Class to manage the highlight effect on an object
 */
public class Highlightable : MonoBehaviour
{
    // Reference to the highlight GameObject
    private GameObject _highlightObject;

    
    /*
     * Initialize the highlightable object
     */
    void Awake()
    {
        // get the child object named "Highlight" and put it in _highlightObject
        Transform highlightTransform = transform.Find("Highlight");
        if (highlightTransform != null)
        {
            _highlightObject = highlightTransform.gameObject;
        }
        
        // make sure that highlight is disabled at start
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Oggetto 'Highlight' non assegnato nello script Highlightable su: " + gameObject.name);
        }
    }

    
    /*
     * Enable the highlight GameObject to show the highlight effect
     */
    public void Highlight()
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(true);
        }
    }

    
    /*
     * Disable the highlight GameObject to hide the highlight effect
     */
    public void Unhighlight()
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(false);
        }
    }
}