using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BTECUnit4CSHARP
{
    class SampleProgram
    {
        static void Main(string[] args)
        {
            //load up the inventory manager
            InventoryManager m = new InventoryManager();
            do
            {
                m.RunProgram();
            } while (true);
            Console.ReadLine();
        }
    }

    class InventoryManager
    {
        //the list of inventory items
        public InventoryItem[] inventory;
        //the first row of the file data - used for saving and loading
        public string[] headerRow;
        //used to get the new ID to save
        public int newInventoryID = 0;
        //a set of constants for the stock level data
        public const int MAX_STOCK_LEVEL = 90;
        public const int MIN_STOCK_LEVEL = 10;
        public const int REORDER_THRESHOLD = 50;
        //a set of constants containing the location of our files.
        public const string INVENTORY_FILE = "InventoryData.txt";
        public const string ERROR_LOG_FILE = "ErrorLog.txt";
        /// <summary>
        /// record that contains the dat for each inventory item
        /// </summary>
        public struct InventoryItem
        {
            public int ID;
            public string Name;
            public string Category;
            public string Status;
            public int StockLevel;
        }
        /// <summary>
        /// the main entry point to the program.
        /// </summary>
        public void RunProgram()
        {
            try
            {
                //load the inventory data
                LoadInventoryData();
                //display the main menu.
                MainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The following error has occurred: {ex.Message}");
                Console.WriteLine("Please contact your system administrator.");
                LogError(ex, "There was an error in Load Inventory Data");
                Proceed();
            }
        }

        /// <summary>
        /// display the main menu to the program
        /// </summary>
        public void MainMenu()
        {
            int userInput;
            bool validInput = false;
            Console.Clear();//empty the console. Not available in Python.
            Console.WriteLine("Welcome to TheComputingTutor Inventory Management System.\n");
            Console.WriteLine("Please choose from one of the following options:\n");
            Console.WriteLine("Press 1 to view the Inventory Sub Menu");
            Console.WriteLine("Press 2 to view the Inventory Category Submenu\n");
            do
            {
                Console.Write("Please enter your menu choice 1 or 2: ");
                //make sure a number has been entered.
                userInput = CheckNumericalUserInput();
                //work out the input
                switch (userInput)
                {
                    case 1://run inventory sub menu
                        validInput = true;
                        DisplayInventorySubMenu();
                        break;
                    case 2:
                        validInput = true;
                        //display the categories
                        DisplayCategorySubMenu();
                        Proceed();
                        //call the Main Menu again.
                        MainMenu();
                        break;
                    default://check for user error.
                        validInput = false;
                        Console.WriteLine("This is not a valid input.\nPlease Try Again:");
                        break;
                }
            } while (!validInput);
        }

        /// <summary>
        /// function that loads the inventory data from a file.
        /// creates the list of InventoryItems
        /// this list is available to the whole program.
        /// </summary>
        public void LoadInventoryData()
        {
            //create the holding array as normal
            int numberOfItemsInInventory = GetNumberOfInventoryItems();
            string[] tempData = new string[numberOfItemsInInventory];
            //create a streamreader object
            if (File.Exists(INVENTORY_FILE))
            {
                //the file exists, so load it.
                StreamReader sr = new StreamReader(INVENTORY_FILE);
                //make sure this is in a try catch block.
                try
                {
                    int counter = 0;
                    //keep doing this while we are NOT at the end of the file.
                    while (!sr.EndOfStream)
                    {
                        //read one line of data into the array
                        tempData[counter] = sr.ReadLine();
                        //increment the counter
                        counter++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred in loading the file data");
                    LogError(ex, "An error occurred in loading the file data");
                }
                //and shut down streamreader
                finally
                {
                    sr.Close();
                }
            }
            else
            {
                LogError("The Inventory file could not be found");
                throw new Exception("The Inventory File could not be found");
            }
            //once the file has been loaded, build the data list
            BuildInventory(tempData);
        }

        /// <summary>
        /// this will build the list of inventory items
        /// thie list will be available to the entire program
        /// </summary>
        /// <param name="fileData">the string array loaded from the file.</param>
        public void BuildInventory(string[] fileData)
        {
            //create the inventory
            inventory = new InventoryItem[fileData.Length - 1];
            //deal with the header row
            headerRow = fileData[0].Split(',');
            try
            {
                //loop through the rest of the file
                for (int i = 1; i < fileData.Length; i++)
                {
                    //create an inventory item
                    InventoryItem temp = new InventoryItem();
                    //split up the file row based on the comma
                    string[] tempItem = fileData[i].Split(',');
                    //set all the values for the item
                    temp.ID = int.Parse(tempItem[0]);
                    temp.Name = tempItem[1];
                    temp.Category = tempItem[2];
                    temp.StockLevel = int.Parse(tempItem[3]);
                    temp.Status = tempItem[4];
                    //add the item to the inventory
                    inventory[i - 1] = temp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred building the inventory list");
                LogError(ex, "build inventory");
            }
        }

        /// <summary>
        /// function that displays the inventory sub menu and handles the user input
        /// </summary>
        public void DisplayInventorySubMenu()
        {
            int userChoice;
            bool validInput = false;
            Console.Clear();
            DisplayFullInventory();
            Console.WriteLine("What would you like to do now?");
            Console.WriteLine("Press 1 to edit stock levels.");
            Console.WriteLine("Press 2 to add a new stock item.");
            Console.WriteLine("Press 3 to save the inventory.");
            Console.WriteLine("Press 4 to go back to the Main Menu.\n");
            do
            {
                Console.Write("Please enter your menu choice 1, 2, 3 or 4: ");
                //again make sure the input is a number
                userChoice = CheckNumericalUserInput();
                switch (userChoice)
                {
                    case 1:
                        validInput = true;
                        EditStockLevels();
                        break;
                    case 2:
                        validInput = true;
                        AddNewStockItem();
                        break;
                    case 3:
                        validInput = true;
                        Console.WriteLine("You are saving the data to a file.");
                        Proceed();
                        SaveDataToFile();
                        break;
                    case 4:
                        validInput = true;
                        Proceed();
                        MainMenu();
                        break;
                    default:
                        validInput = false;
                        Console.WriteLine("This is not a valid input.\nPlease Try Again:");
                        break;
                }
            } while (!validInput);
        }

        /// <summary>
        /// function to change the stock levels of a specific item.
        /// The item must first be located by ID or Name.
        /// Then the item levels are decreased.
        /// </summary>
        private void EditStockLevels()
        {
            string itemName = "";
            string haveAnotherGo = "";
            int itemID = 0, stockLevel = 0;
            bool isID = false, validInput = false, itemFound = false, goAgain = false;
            //the item we are looking for.
            InventoryItem inventoryTarget = new InventoryItem();
            do
            {//outer loop - allow a user to change more than one stock level.
                Console.Clear();
                //firstly display the inventory to the user
                DisplayFullInventory();
                do
                { //first loop - check the stock input
                    Console.Write("Please enter the ID or the Name of the item:");
                    try
                    {
                        itemName = Console.ReadLine();
                        itemID = int.Parse(itemName);
                        isID = true;
                    }
                    //if the value is not a number
                    catch (FormatException ex)
                    {
                        //set this to false.
                        isID = false;
                    }
                    //if a number has been entered
                    if (isID)
                    {
                        //search for the matching ID
                        foreach (InventoryItem i in inventory)
                        {
                            if (i.ID == itemID)
                            {
                                inventoryTarget = i;
                                itemFound = true;
                            }
                        }
                    }
                    else
                    {//search for the matching name
                        foreach (InventoryItem i in inventory)
                        {
                            if (i.Name == itemName)
                            {
                                inventoryTarget = i;
                                itemFound = true;
                            }
                        }
                    }
                    //if no match, keep looping.
                    if (!itemFound)
                    {
                        Console.WriteLine("This item does not exist.");
                        Console.WriteLine("Please check your spelling or that you have typed the right ID");
                    }
                } while (!itemFound);
                Console.Write("By how much has your stock changed: ");
                do
                {//check the stock level input
                    stockLevel = CheckNumericalUserInput();
                    if (stockLevel == -1)
                    {
                        Console.WriteLine("this is not a valid user input.\nPlease try again.");
                    }
                } while (stockLevel == -1);
                //at this point there is a valid number
                //add the numbers - negatives will be dealt with.
                int newStockLevel = inventoryTarget.StockLevel + stockLevel;
                //check maximum
                if (newStockLevel >= MAX_STOCK_LEVEL)
                {
                    Console.WriteLine("You are at maximum stock level");
                    Console.WriteLine("no need to do anything");
                    validInput = true;
                }
                //check minimum
                else if (newStockLevel <= MIN_STOCK_LEVEL)
                {
                    Console.WriteLine("ALERT! Your stock is below the minimum required level!");
                    inventoryTarget.StockLevel = newStockLevel;
                    inventoryTarget.Status = "ALERT!";
                    validInput = true;
                }
                //check general.
                else if (newStockLevel > MIN_STOCK_LEVEL && newStockLevel <= REORDER_THRESHOLD)
                {
                    Console.WriteLine("WARNING! Your stock is running low. Reorder!");
                    inventoryTarget.StockLevel = newStockLevel;
                    inventoryTarget.Status = "ORDER!";
                    validInput = true;
                }
                else
                {
                    Console.WriteLine("Your Stock levels for this item are OK.");
                    inventoryTarget.StockLevel = newStockLevel;
                    inventoryTarget.Status = "GOOD";
                    validInput = true;
                }
                //update the inventory with the new values
                for (int i = 0; i < inventory.Length; i++)
                {
                    if (inventory[i].ID == inventoryTarget.ID)
                    {
                        //change the values that have changed in the inventry
                        inventory[i].StockLevel = inventoryTarget.StockLevel;
                        inventory[i].Status = inventoryTarget.Status;
                    }
                }
                //clear and display
                Console.Clear();
                DisplayFullInventory();
                do
                {//deal with what happens next
                    Console.WriteLine("Would you like to edit another item?");
                    Console.Write("Press Y to continue or N to go back to the inventory Sub Menu: ");
                    haveAnotherGo = Console.ReadLine().ToLower();
                    if (haveAnotherGo == "y")
                    {
                        validInput = true;
                        goAgain = true;
                    }
                    else if (haveAnotherGo == "n")
                    {
                        validInput = true;
                        goAgain = false;
                        Console.Clear();
                        DisplayInventorySubMenu();
                    }
                    else
                    {
                        validInput = false;
                        Console.WriteLine("this is not a valid input, please try again.\n");
                        goAgain = false;
                    }
                } while (!validInput);
            } while (goAgain);
        }
        /// <summary>
        /// code for adding a new item. The status and stock level will be set to defaults.
        /// </summary>
        private void AddNewStockItem()
        {
            string itemName, userInput;
            int itemCategory;
            string itemStatus = "MAX";
            int itemStockLevel = 100;
            bool validCategory = false, validName = true;
            InventoryItem newItem = new InventoryItem();
            Console.Clear();
            Console.WriteLine("ADD NEW INVENTORY ITEM:\n");
            do
            {
                Console.Write("Please enter the name of the item: ");
                itemName = Console.ReadLine();
                //check to make sure the item doesn't exist
                foreach (InventoryItem i in inventory)
                {
                    if (i.Name == itemName)
                    {
                        Console.WriteLine("This item name already exists");
                        Console.WriteLine("Please try again.");
                        validName = false;
                    }
                }
            } while (!validName);
            //get the list of categories
            string[] categories = GetCategories();
            Console.WriteLine("Please choose a category: ");
            //now print out the details at the end
            DisplayCategories(categories);
            do
            {
                Console.Write("\n\nPlease enter the category of the item: ");
                itemCategory = CheckNumericalUserInput();
                //a brief check to make sure the number makes sense.
                if (itemCategory < 0 || itemCategory > categories.Length)
                {
                    Console.Write("\nThis is not a valid category.\nPlease try again.");
                    validCategory = false;
                }
                else
                {
                    validCategory = true;
                    newItem.Category = categories[itemCategory - 1];
                }
            } while (!validCategory);
            newItem.ID = newInventoryID;
            newItem.Name = itemName;
            newItem.Status = itemStatus;
            newItem.StockLevel = itemStockLevel;
            //create the new inventory of old size + 1
            InventoryItem[] newInventory = new InventoryItem[inventory.Length + 1];
            //copy the inventory over
            inventory.CopyTo(newInventory, 0);
            //add the new item
            newInventory[inventory.Length] = newItem;
            //reset the inventory with the new one
            inventory = newInventory;
            DisplayFullInventory();
            do
            {
                Console.WriteLine("Would you like to save the new inventory data?");
                Console.Write("Press Y to save the data or N to return to the main menu: ");
                userInput = Console.ReadLine().ToLower();
                if (userInput != "n" && userInput != "y")
                {
                    Console.WriteLine("this is not a valid input, please try again.");
                }
            } while (userInput != "n" && userInput != "y");
            if (userInput == "n")
            {
                MainMenu();
            }
            else
            {
                SaveDataToFile();
                Proceed();
            }
        }
        /// <summary>
        /// function that will save the inventory back the the file.
        /// </summary>
        private void SaveDataToFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter(INVENTORY_FILE, false);
                string output = "";
                //build the header row first
                for (int i = 0; i < headerRow.Length; i++)
                {
                    if (i == headerRow.Length - 1)
                    {
                        output = output + $"{headerRow[i]}";
                    }
                    else
                    {
                        output = output + $"{headerRow[i]}, ";
                    }
                }
                // then save it
                sw.WriteLine(output);
                //then save the inventory
                for (int i = 0; i < inventory.Length; i++)
                {
                    sw.WriteLine($"{inventory[i].ID} , {inventory[i].Name}, {inventory[i].Category}, {inventory[i].StockLevel}, {inventory[i].Status}");
                }
                Console.WriteLine("File Data Saved Sucessfully.");
                //close the streamwriter
                sw.Close();
            }
            catch
            {
                Console.WriteLine("there was a problem creating the error file.");
                Console.WriteLine("please contact your sytem administrator");
            }
        }
        #region functions that deal with display of data only
        /// <summary>
        /// a function that checks the inventory to get the number of items
        /// this is used to build the inventory array with the correct number
        /// it is also used to get the next ID if a new item is to be added to the inventory.
        /// </summary>
        /// <returns>int number of items in the inventory</returns>
        private int GetNumberOfInventoryItems()
        {
            int counter = 0;
            if (File.Exists(INVENTORY_FILE))
            {
                //create a streamreader object
                StreamReader sr = new StreamReader(INVENTORY_FILE);
                //make sure this is in a try catch block.
                try
                {
                    //keep doing this while we are NOT at the end of the file.
                    while (!sr.EndOfStream)
                    {
                        //read one line of data into the array
                        sr.ReadLine();
                        //increment the counter
                        counter++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred loading the Inventory Data");
                    LogError(ex, "counting number of inventory items");
                }
                //and shut down streamreader
                finally
                {
                    sr.Close();
                }
            }
            else
            {
                LogError("The Inventory file could not be found");
                throw new Exception("The Inventory File could not be found");
            }
            //set up the new ID
            newInventoryID = counter;
            //return the number of items in the list.
            return counter;
        }
        /// <summary>
        /// function that displays the inventory list in a grid format
        /// this will display ONLY the items with the matching category.
        /// </summary>
        private void DisplayInventoryFilteredByCategory(string category)
        {
            Console.WriteLine($"\n\nDisplaying inventory of {category} items");
            PrintSeparator(true);
            //deal with the header row
            for (int i = 0; i < headerRow.Length; i++)
            {
                //put tabs in everywhere to make sure it displays properly
                Console.Write($"|\t{headerRow[i].Trim()}\t");
            }
            Console.Write("|");
            PrintSeparator(true);
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].Category == category)
                {
                    InventoryItem temp = inventory[i];
                    string output = $"|\t{temp.ID.ToString().Trim()}\t";
                    //different number of tabs depending on the length of the Name
                    output = output + $"|\t{ temp.Name.Trim()}\t";
                    output = output + $"|\t{ temp.Category.Trim()}\t";
                    output = output + $"|\t{ temp.StockLevel.ToString().Trim()}\t |\t{ temp.Status.Trim()}\t |";
                    //write the output
                    Console.Write(output);
                    PrintSeparator(true);
                }
            }
        }

        /// <summary>
        /// function that displays the inventory list in a grid format
        /// </summary>
        private void DisplayFullInventory()
        {
            Console.WriteLine("Displaying Full Inventory....");
            PrintSeparator(true);
            //deal with the header row
            for (int i = 0; i < headerRow.Length; i++)
            {
                Console.Write($"|\t{headerRow[i].Trim()}\t");
            }
            Console.Write("|");
            PrintSeparator(true);
            for (int i = 0; i < inventory.Length; i++)
            {
                InventoryItem temp = inventory[i];
                //build up the output string
                string output = $"|\t{temp.ID.ToString().Trim()}\t";
                output = output + $"|\t{ temp.Name.Trim()}\t";
                output = output + $"|\t{ temp.Category.Trim()}\t";
                output = output + $"|\t{ temp.StockLevel.ToString().Trim()}\t |\t{ temp.Status.Trim()}\t |";
                //write the output
                Console.Write(output);
                PrintSeparator(true);
            }
        }
        /// <summary>
        /// menu function that handles the user input for the category sub menu
        /// </summary>
        public void DisplayCategorySubMenu()
        {
            int userInput;
            bool validInput = false;
            Console.Clear();//empty the console. Not available in Python.
            Console.WriteLine("Please choose from one of the following options:\n");
            Console.WriteLine("Press 1 to view the Available Categories.");
            Console.WriteLine("Press 2 to list all items by Category.");
            Console.WriteLine("Press 3 to return to the Main Menu.\n");
            do
            {
                Console.Write("Please enter your menu choice 1, 2 or 3: ");
                //make sure a number has been entered.
                userInput = CheckNumericalUserInput();
                //work out the input
                switch (userInput)
                {
                    case 1://run inventory sub menu
                        validInput = true;
                        DisplayCategories();
                        Proceed();
                        DisplayCategorySubMenu();
                        break;
                    case 2:
                        validInput = true;
                        //display the categories
                        ListItemsByCategory();
                        Proceed();
                        //call the Main Menu again.
                        DisplayCategorySubMenu();
                        break;
                    case 3:
                        validInput = true;
                        MainMenu();
                        break;
                    default://check for user error.
                        validInput = false;
                        Console.WriteLine("This is not a valid input.\nPlease Try Again:");
                        break;
                }
            } while (!validInput);
        }

        /// <summary>
        /// a function that goes and gets the list of categories
        /// and prints them out
        /// </summary>
        private void DisplayCategories()
        {
            string[] categories = GetCategories();
            Console.WriteLine("-----------------------------------------------------");
            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i] != null)
                {
                    Console.WriteLine($"|\tCategory ID {i + 1} \t|\t {categories[i]}\t|");
                    Console.WriteLine("---------------------------------------------------");
                }
            }
        }

        /// <summary>
        /// function that will list all inventory items belonging to a selected category
        /// </summary>
        private void ListItemsByCategory()
        {
            int userInput;
            bool validInput = false;
            //get the list of categories
            string[] categories = GetCategories();
            string searchCategory;
            Console.Clear();
            //show the categories to the user.
            DisplayCategories();
            do
            {
                //handle the input for the category
                Console.Write("Please enter a categoryID: ");
                userInput = CheckNumericalUserInput();
                if (userInput == -1)
                {
                    Console.WriteLine("This is not a valid input. Please try again.\n");
                    validInput = false;
                }
                else if (userInput < 0 || userInput > categories.Length)
                {
                    Console.WriteLine("This is not a valid Category. Please try again.\n");
                    validInput = false;
                }
                else
                {
                    validInput = true;
                }
            } while (!validInput);
            //ID the selected category
            searchCategory = categories[userInput - 1];
            Console.WriteLine();
            DisplayInventoryFilteredByCategory(searchCategory);
        }

        /// <summary>
        /// a function that loops through the supplied list of categories and prints them out
        /// </summary>
        /// <param name="categories"></param>
        private void DisplayCategories(string[] categories)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i] != null)
                {
                    Console.WriteLine($"Press {i + 1} for {categories[i]}");
                }
            }
        }
        /// <summary>
        /// function that gets a list of the unique categories of items
        /// and adds them to an array
        /// the array will only store the correct number of items
        /// </summary>
        /// <returns></returns>
        private string[] GetCategories()
        {
            //a temp array to hold all the items
            string[] tempCategories = new string[inventory.Length];
            //this will hold all our unique categories
            string[] categories;
            //keep a track on the number of matches
            int checkedCounter = 0;
            //how many categories we have
            int matchingCategories = 0;
            for (int i = 0; i < inventory.Length; i++)
            {
                //int matchCounter = 1;
                //get the item
                InventoryItem temp = inventory[i];
                bool exists = false;
                //finally loop through the checked item list to make sure the item hasn't already been added
                foreach (string s in tempCategories)
                {
                    //if it has, we don't add them again.
                    if (s == temp.Category)
                    {
                        exists = true;
                    }
                }
                //if not- add them.
                if (!exists)
                {
                    //add them to the list
                    tempCategories[checkedCounter] = temp.Category;
                    //and keep track of the number of items
                    matchingCategories++;
                    checkedCounter++;
                }
            }
            //now create the actual array - this will have only the right number of elements
            categories = new string[matchingCategories];
            int counter = 0;
            //loop through the temp list
            for (int i = 0; i < tempCategories.Length; i++)
            {
                if (tempCategories[i] != null)
                {
                    //add the matching items in the right location
                    categories[counter] = tempCategories[i];
                    //incrememnt the counter only when we have a valid input
                    counter++;
                }
            }
            return categories;
        }
        /// <summary>
        /// simple function to print the horizontal seprator for table based displays
        /// </summary>
        /// <param name="withNewline">add a new line character at the end</param>
        private void PrintSeparator(bool withNewline)
        {
            if (withNewline)
            {
                Console.WriteLine("\n-------------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("------------------------------------------------------------------------------------------------");
            }
        }
        #endregion
        /// <summary>
        /// used extensively to check a number has been entered.
        /// This means a TryParse function is not needed
        /// </summary>
        /// <returns>returns -1 if the user enters a word</returns>
        private int CheckNumericalUserInput()
        {
            int userInput = 0;
            try
            {
                userInput = int.Parse(Console.ReadLine());
            }
            //if the user enters a word, this will throw an error
            catch (FormatException ex)
            {
                //set the input to -1
                userInput = -1;
            }
            return userInput;
        }
        /// <summary>
        /// function that will log any custom message to the log file.
        /// </summary>
        /// <param name="message">any custom message</param>
        /// <returns>true if the operation was successful</returns>
        private bool LogError(string message)
        {
            bool logSuccessful = false;
            try
            {
                StreamWriter sw = new StreamWriter("ErrorLog.txt", true);
                sw.WriteLine($"{System.DateTime.Now}: ERROR {message}");
                sw.Close();
            }
            catch
            {
                Console.WriteLine("there was a problem creating the error file.");
                Console.WriteLine("please contact your sytem administrator");
            }
            return logSuccessful;
        }
        /// <summary>
        /// function that will log any exception and custom message to the log file.
        /// </summary>
        /// <param name="ex">the exception that caused the error</param>
        /// <param name="message">any custom message</param>
        /// <returns>true if the operation was successful</returns>
        private bool LogError(Exception ex, string message)
        {
            bool logSuccessful = false;
            try
            {
                StreamWriter sw = new StreamWriter("ErrorLog.txt", true);
                sw.WriteLine($"{System.DateTime.Now}: ERROR {message}");
                sw.WriteLine($"{System.DateTime.Now}: Exception {ex.Message}");
                sw.WriteLine($"{System.DateTime.Now}: STACK TRACE {ex.StackTrace}");
                sw.Close();
            }
            catch
            {
                Console.WriteLine("there was a problem creating the error file.");
                Console.WriteLine("please contact your system administrator");
            }
            return logSuccessful;
        }
        /// <summary>
        /// simple function to present an interface to the user
        /// clears the screen for the next function call
        /// </summary>
        private void Proceed()
        {
            Console.WriteLine("\n\nPress any key to continue....");
            Console.ReadLine();
            Console.Clear();
        }
    }
}