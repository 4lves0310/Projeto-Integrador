using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;
<<<<<<< HEAD
    
=======
    // Start is called before the first frame update
>>>>>>> 8eede34eea5137a3ac19d749beebddb7174ad3a2
    void Start()
    {
        ActivateTab(0);
    }

<<<<<<< HEAD
    public void ActivateTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false); 
        }
        
        pages[tabNo].SetActive(true); 
    }
}
=======
    // Update is called once per frame
    public void ActivateTab(int tabNo)
    {
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].SetActive(false);
                tabImages[i].color = Color.grey;
            }
            pages[tabNo].SetActive(true);
            tabImages[tabNo].color = Color.white;
        }
    }
}
>>>>>>> 8eede34eea5137a3ac19d749beebddb7174ad3a2
