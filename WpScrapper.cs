using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenQA.Selenium.Interactions;
public class WpScrapper
{

	ChromeDriver driver;
	ChromeOptions options;
    List<tInfo> iphoneMini = new List<tInfo>();
    List<tInfo> iphoneRegular = new List<tInfo>();
    List<tInfo> iphonePro = new List<tInfo>();
    float avgMiniPrice = 0;
    float avgRegularPrice = 0;
    float avgProPrice = 0;
    string itemToSearch = null;
    string filterXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/div/tsl-sort-filter/div/div[1]/div[2]";
    string sortByPriceXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/div/tsl-sort-filter/div/div[2]/div[2]/tsl-select-form/div[3]/tsl-select-option/div/div/span";
    int elementsToFind = 0;

    struct tInfo
    {
        public string _item;
        public string _itemURL;
        public float _price;
    }
	//INSERT THE WEBPAGE IN launchWebsite() !!!!
    public WpScrapper(string itemToSearch, int elementsToFind)
	{
        this.itemToSearch = itemToSearch;
        this.elementsToFind = elementsToFind;

        int minimumPrice = 40;
        string searchBarXPath = "//*[@id=\"searchBoxForm\"]/div/div[1]/input[1]";
        string seeMoreXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/div[2]/tsl-button/button";
        //*[@id="btn-load-more"]/button
        string nameXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + 1 + "]/tsl-public-item-card/div/div[3]/div[2]/p";
        string nameXPathOpt2 = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + 1 + "]/tsl-public-item-card/div/div[4]/div[2]/p";
        string priceXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + 1 + "]/tsl-public-item-card/div/div[3]/div[1]/div/span";
        string priceXPathOpt2 = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + 1 + "]/tsl-public-item-card/div/div[4]/div[1]/div/span";
        string urlXPath = "";

        launchWebsite();

        IWebElement search = driver.FindElement(By.XPath(searchBarXPath));
        search.Click();
        search.Clear();
        search.SendKeys(itemToSearch);
        driver.FindElement(By.XPath("//*[@id=\"searchBoxForm\"]/a")).Click();

        

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(nameXPath)));
        prepareWebsite();
        int itemIndex = 1;
        
        while (this.iphoneMini.Count + this.iphoneRegular.Count + this.iphonePro.Count < elementsToFind)
        {
            try
            {
                nameXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + itemIndex + "]/tsl-public-item-card/div/div[3]/div[2]/p";
                nameXPathOpt2 = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + itemIndex + "]/tsl-public-item-card/div/div[4]/div[2]/p";
                priceXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + itemIndex + "]/tsl-public-item-card/div/div[3]/div[1]/div/span";
                priceXPathOpt2 = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div[1]/tsl-public-item-card-list/div/a[" + itemIndex + "]/tsl-public-item-card/div/div[4]/div[1]/div/span";
                urlXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/tsl-search-layout/div/div[2]/div/tsl-public-item-card-list/div/a[[" + itemIndex + "]/tsl-public-item-card/div/div[3]";

                int opt2 = 0;
                string name = getNameWithXPath(driver, nameXPath);
                if (name == null)
                {
                    pressLoadMoreButton(driver, seeMoreXPath);
                    name = getNameWithXPath(driver, nameXPath);
                    if (name == null)
                    {
                        name = getNameWithXPath(driver, nameXPathOpt2);
                        if (name == null)
                        {
                            pressLoadMoreButton(driver, seeMoreXPath);
                            name = getNameWithXPath(driver, nameXPathOpt2);
                        }
                    }
                }
                if (name != null)
                {
                    string price = "";
                    if (opt2 == 0)
                    {
                        Console.WriteLine("Default div: 0");
                        price = driver.FindElement(By.XPath(priceXPath)).Text;
                    }
                    else
                    {
                        Console.WriteLine("Secondary div: 1");
                        price = driver.FindElement(By.XPath(priceXPathOpt2)).Text;
                    }
                    price = price.Remove(price.Length - 1); //Remove euro char

                    float asd = convertPrice(price);
                    if (asd >= minimumPrice)
                    {
                        string chatXPath = "/html/body/div[2]/div[2]/div[2]/div[2]/div[1]/div[1]/div/div[3]/a";

                        driver.FindElement(By.XPath(priceXPath)).Click();
                        driver.SwitchTo().Window(driver.WindowHandles[1]);

                        WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                        IWebElement element2 = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(chatXPath)));
                        string url = driver.Url;
                        driver.Close();
                        driver.SwitchTo().Window(driver.WindowHandles[0]);
                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                        element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(nameXPath)));
                        tInfo product;
                        product._item = name;
                        product._itemURL = url;
                        product._price = asd;
                        addProductToList(product, iphoneMini, iphoneRegular, iphonePro, ref avgMiniPrice, ref avgRegularPrice, ref avgProPrice);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Error: " + itemIndex);
            }
            ++itemIndex;
        }

        bubbleSort(iphoneMini);
        bubbleSort(iphoneRegular);
        bubbleSort(iphonePro);
        listItems();
        Console.WriteLine();
        Console.WriteLine("Avg Mini Price: " + avgMiniPrice / iphoneMini.Count);
        Console.WriteLine("Avg Regular Price: " + avgRegularPrice / iphoneRegular.Count);
        Console.WriteLine("Avg Pro Price: " + avgProPrice / iphonePro.Count);
        //Thread.Sleep(5000);
        //driver.Close();

    }

    private void launchWebsite()
    {
        this.options = new ChromeOptions();
        options.AddArgument(@"user-data-dir=C:\Users\javie\AppData\Local\Google\Chrome\User Data\Default");
        this.driver = new ChromeDriver(options);
		//INSERT THE WEBPAGE HERE!!
        driver.Navigate().GoToUrl("");
        var window = driver.Manage().Window;
        window.Maximize();
    }

    private void prepareWebsite()
    {
        driver.FindElement(By.XPath(this.filterXPath)).Click();
        driver.FindElement(By.XPath(this.sortByPriceXPath)).Click();
        Thread.Sleep(2000); //Load the website...
        //Click on the price
        driver.FindElement(By.XPath("/html/body/tsl-root/tsl-public/div/div/tsl-search/div/div/tsl-filters-wrapper/div/div/tsl-filter-group/div/div[4]/tsl-filter-host/div/tsl-range-filter/tsl-filter-template/div/tsl-bubble/div/div/div")).Click();
        //Move the slider
        string sliderSelector = "body > tsl-root > tsl-public > div > div > tsl-search > div > div > tsl-filters-wrapper > div > div.FiltersWrapper__bar.d-flex.pl-3.py-2.FiltersWrapper__bar--opened > tsl-filter-group > div > div:nth-child(4) > tsl-filter-host > div > tsl-range-filter > tsl-filter-template > div > div > div.FilterTemplate__filter.px-4 > div > form > tsl-slider-form > form > ngx-slider > span.ngx-slider-span.ngx-slider-pointer.ngx-slider-pointer-min";
        IWebElement Slider = driver.FindElement(By.CssSelector(sliderSelector));
        Actions move = new Actions(driver);
        move.MoveToElement(Slider).ClickAndHold().MoveByOffset(200, 250).Release().Perform();
        //Click on apply
        string applyXPath = "/html/body/tsl-root/tsl-public/div/div/tsl-search/div/div/tsl-filters-wrapper/div/div[2]/tsl-filter-group/div/div[4]/tsl-filter-host/div/tsl-range-filter/tsl-filter-template/div/div/div[3]/tsl-button[2]/button";
        driver.FindElement(By.CssSelector("body > tsl-root > tsl-public > div > div > tsl-search > div > div > tsl-filters-wrapper > div > div.FiltersWrapper__bar.d-flex.pl-3.py-2.FiltersWrapper__bar--opened > tsl-filter-group > div > div:nth-child(4) > tsl-filter-host > div > tsl-range-filter > tsl-filter-template > div > div > div.FilterTemplate__actions.px-4.d-flex.align-items-center.justify-content-end > tsl-button.mx-1.ng-star-inserted > button")).Click();
    }

    private void printItem(tInfo product, int itemIndex)
    {
        Console.WriteLine("[" + itemIndex + "] " + product._price + "\t " + product._item + "\t " + product._itemURL);
    }

    private void bubbleSort(List<tInfo> products)
    {
        //Sorting by Price: Bubble Sort
        var n = products.Count;
        for (int i = 0; i < n - 1; i++)
            for (int j = 0; j < n - i - 1; j++)
                if (products[j]._price > products[j + 1]._price)
                {
                    var tempVar = products[j];
                    products[j] = products[j + 1];
                    products[j + 1] = tempVar;
                }
    }

    private void listItems()
    {
        Console.WriteLine();
        Console.WriteLine("iPhones Mini: \n");
        for (int i = 0; i < iphoneMini.Count; ++i) printItem(iphoneMini[i], i);
        Console.WriteLine();
        Console.WriteLine("iPhones Regular: \n");
        for (int i = 0; i < iphoneRegular.Count; ++i) printItem(iphoneRegular[i], i);
        Console.WriteLine();
        Console.WriteLine("iPhones Pro: \n");
        for (int i = 0; i < iphonePro.Count; ++i) printItem(iphonePro[i], i);
    }

    private string getNameWithXPath(ChromeDriver driver, string nameXPath)
    {
        string name = null;
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(nameXPath)));
        try //FIND ELEMENT
        {
            IWebElement iElement = iElement = driver.FindElement(By.XPath(nameXPath));
            name = iElement.Text;
            Actions actions = new Actions(driver);
            actions.MoveToElement(iElement);
            actions.Perform();
            return name;
        }
        catch (Exception e)
        {
            Console.WriteLine("couldnt find the desired name");
            return null;
        }
    }

    private void pressLoadMoreButton(ChromeDriver driver, string seeMoreXPath)
    {
        try
        {
            Console.WriteLine("pressing the load button");
            IWebElement moreButton = driver.FindElement(By.XPath(seeMoreXPath));
            Actions actions = new Actions(driver);
            actions.MoveToElement(moreButton);
            actions.Perform();
            moreButton.Click();
        }
        catch (Exception ex)
        {
            Console.Out.WriteLine("didnt find load more button!");
        }
    }

    private float convertPrice(string price)
    {
        float asd = 0;
        try
        {
            asd = (float)Convert.ToDouble(price);
        }
        catch (Exception e)
        {
            Console.WriteLine("couldnt convert price into float...");
        }
        return asd;
    }

    private string determineProduct(string name)
    {
        if (name.ToUpperInvariant().Contains(this.itemToSearch.ToUpperInvariant() + " MINI")) return "mini";
        else if (name.ToUpperInvariant().Contains(this.itemToSearch.ToUpperInvariant() + " PRO")) return "pro";
        else if (name.ToUpperInvariant().Contains(this.itemToSearch.ToUpperInvariant())) return "regular";
        else return "";
    }

    private void addProductToList(tInfo item, List<tInfo> iphoneMini, List<tInfo> iphoneRegular, List<tInfo> iphonePro, ref float avgMiniPrice, ref float avgRegularPrice, ref float avgProPrice)
    {
        string type = determineProduct(item._item);
        if (type == "mini")
        {
            avgMiniPrice += item._price;
            iphoneMini.Add(item);
        }
        if (type == "regular")
        {
            avgRegularPrice += item._price;
            iphoneRegular.Add(item);
        }
        if (type == "pro")
        {
            avgProPrice += item._price;
            iphonePro.Add(item);
        }
    }

}
