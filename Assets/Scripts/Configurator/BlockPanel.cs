using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BlockPanel : MonoBehaviour
{
    /*
     * This script is attached to BlockPanel.
     * 
     * Handles property selected, updating property values, and populating list of properties.
     */


    public GameObject PropertySelectionDropdown;
    public GameObject BlockParameterText;
    public GameObject TextInputField;
    public GameObject DropdownInputField;
    public GameObject BlockParameterValue;
    public GameObject UIManager;
    public GameObject BlockInfoText;

    private int index = -1;
    public string selectedParameter;
    public string hoveredParameter;

    private ConfigurationUIManager uiManager;

    public void Start()
    {
        uiManager = UIManager.GetComponent<ConfigurationUIManager>();

        //Disables property dropdown, property value input field, etc.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Populates properties dropdown
    public void Populate(int index)
    {
        this.index = index;

        BlockInfoText.GetComponent<TextMeshProUGUI>().text = "Block Properties:\n\n";

        // Adds all properties with "per_block" to the dropdown options
        int i = 0;
        List<string> options = new List<string>();
        foreach (KeyValuePair<string, object> kp in uiManager.ExpContainer.Data)
        {
            if (kp.Key.StartsWith("per_block"))
            {
                options.Add(kp.Key);
                i++;
            }
        }
        PropertySelectionDropdown.GetComponent<Dropdown>().ClearOptions();
        PropertySelectionDropdown.GetComponent<Dropdown>().AddOptions(options);

        UpdateBlockPropertyText();

        // Selects the first property to be displayed by default
        if (options.Count > 0)
        {
            OnClickOption(0);
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    // When selecting a property (from the dropdown, or from the list on the right side of the screen)
    public void OnClickOption(int option)
    {
        //Debug.Log(option);
        selectedParameter = PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text;
        //Debug.Log(selectedParameter);
        BlockParameterText.GetComponent<Text>().text = selectedParameter;
        BlockParameterValue.GetComponent<Text>().text = "Value: " +
                                                        (uiManager.ExpContainer.Data[selectedParameter] as List<object>)[index];

        // Sets up dropdown of values if applicable, otherwise enables text field
        if (uiManager.ExpContainer.GetDefaultValue(
            PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text) is IList)
        {
            TextInputField.SetActive(false);
            DropdownInputField.SetActive(true);

            // Set up options for dropdown
            List<object> list = uiManager.ExpContainer.GetDefaultValue(
                PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text) as List<object>;

            List<string> newList = new List<string>();

            // First option is blank
            newList.Add("");

            foreach (object o in list)
            {
                newList.Add((string)o);
            }

            DropdownInputField.GetComponent<Dropdown>().ClearOptions();
            DropdownInputField.GetComponent<Dropdown>().AddOptions(newList);
        }
        else
        {
            TextInputField.SetActive(true);
            TextInputField.GetComponent<InputField>().text = "";
            DropdownInputField.SetActive(false);
        }

        UpdateBlockPropertyText();
    }

    // Called from properties list on the right side of screen
    // Sets hoveredParameter, then calls UpdateBlockPropertyText to update the colour/highlight
    public void OnHoverOption(int option)
    {
        if (option < 0)
        {
            hoveredParameter = string.Empty;
        }
        else
        {
            hoveredParameter = PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text;
        }
       
        UpdateBlockPropertyText();
    }

    // Called when the property value input field is updated
    // Updates the selected property with the newly inputted value
    public void OnInputFinishEdit(string text)
    {
        if (index == -1 || text.Length == 0) return;

        UndoRedo.instance.Backup();

        object obj = uiManager.ExpContainer.ConvertToCorrectType(text);

        bool isCorrectType = false;

        //Checks if inputted value matches the correct type for that property
        switch(uiManager.ExpContainer.GetDefaultValue(selectedParameter))
        {
            case "":
                isCorrectType = true;
                break;

            case 0:
                if (obj.GetType().IsInstanceOfType(0))
                    isCorrectType = true;
                break;

            case false:
                if (obj.GetType().IsInstanceOfType(false))
                    isCorrectType = true;
                break;

            case 0.0f:
                if (obj.GetType().IsInstanceOfType(0) || obj.GetType().IsInstanceOfType(0.0f))
                    isCorrectType = true;

                break;
            default:
                break;
        }


        if (isCorrectType)
        {
            BlockParameterValue.GetComponent<Text>().text = "Value: " + text;

            ConfigurationBlockManager blockManager = uiManager.BlockView.GetComponent<ConfigurationBlockManager>();
            foreach (GameObject g in blockManager.SelectedBlocks)
            {
                ((List<object>)uiManager.ExpContainer.Data[selectedParameter])[g.GetComponent<BlockComponent>().BlockID] = obj;
            }
            UpdateBlockPropertyText();
            uiManager.Dirty = true;
        }
        else
        {
            uiManager.ConfirmationPopup.GetComponent<ConfirmationPopup>().ShowPopup(
                "The input type does not match the correct type for this property.", null);
        }
    }

    // Called when the property value dropdown is updated
    // Updates the selected property with the newly inputted value
    public void OnDropdownFinishEdit(int option)
    {
        // If user selected blank, don't edit the parameter
        if (option == 0) return;

        UndoRedo.instance.Backup();

        BlockParameterValue.GetComponent<Text>().text = "Value: " +
            DropdownInputField.GetComponent<Dropdown>().options[option].text;

        ConfigurationBlockManager blockManager = uiManager.BlockView.GetComponent<ConfigurationBlockManager>();

        foreach (GameObject g in blockManager.SelectedBlocks)
        {
            ((List<object>)uiManager.ExpContainer.Data[selectedParameter])[g.GetComponent<BlockComponent>().BlockID] =
                DropdownInputField.GetComponent<Dropdown>().options[option].text;
        }

        uiManager.BlockView.GetComponent<ConfigurationBlockManager>().ReadjustBlocks();
        UpdateBlockPropertyText();

        uiManager.Dirty = true;
    }

    // Populates list of block properties on the right side of the screen
    // Uses TextMeshPro to make each line clickable with a different ID (i)
    // The ID of each property in this list matches the index of each property in the properties dropdown
    // Changes colour of the text depending if the current property is selected or hovered over
    private void UpdateBlockPropertyText()
    {
        BlockInfoText.GetComponent<TextMeshProUGUI>().text = "Block Properties:\n\n";

        //Adds all properties with "per_block" to the block properties list
        int i = 0;
        foreach (KeyValuePair<string, object> kp in uiManager.ExpContainer.Data)
        {
            if (kp.Key.StartsWith("per_block"))
            {
                if (kp.Key.Equals(selectedParameter))
                    BlockInfoText.GetComponent<TextMeshProUGUI>().text += 
                        "<color=\"black\"><mark><u><link=\"" + i + "\">" + kp.Key + "</color></mark></u> : " + (kp.Value as List<object>)[index] + "</link>\n";
                else if (kp.Key.Equals(hoveredParameter))
                    BlockInfoText.GetComponent<TextMeshProUGUI>().text +=
                        "<color=\"grey\"></mark><u><link=\"" + i + "\">" + kp.Key + "</color></mark></u> : " + (kp.Value as List<object>)[index] + "</link>\n";
                else
                    BlockInfoText.GetComponent<TextMeshProUGUI>().text += 
                        "<u><link=\"" + i + "\">" + kp.Key + "</u> : " + (kp.Value as List<object>)[index] + "</link>\n";
                i++;
            }
        }
    }
}
