using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace ContactForm;

public class ContactFormTests: IDisposable
{
    private IWebDriver driver;
    private WebDriverWait wait;

    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Navigate().GoToUrl("https://www.progress.com/company/contact");
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    [Test]
    public void SubmitFormWithValidDataWithCountryWithoutState()
    {
        FillContactForm("Chef – DevOps", "test@progress.com", "Test First Name", "Test Last Name", "TestCompany",
        "Others", "Bulgaria", "+359877700247", "Test Message");
        SubmitContactForm();
        Assert.That(SubmitSuccessfully(), Is.True, "Success message was not displayed after form submission.");
    }

    [Test]
    public void SubmitFormWithEmptyName()
    {
        FillContactForm("Chef – DevOps", "test@progress.com", "", "Test Last Name", "TestCompany",
        "Others", "Bulgaria", "+359877700247", "Test Message");
        SubmitContactForm();
        Assert.That(IsValidationErrorDisplayed(), Is.EqualTo("First name is required"), "Is Required error message is not displayed correctly!");
    }

    [Test]
    public void SubmitFormWithInvalidEmail()
    {
        FillContactForm("Chef – DevOps", "testprogress.com", "Test First Name", "Test Last Name", "TestCompany",
        "Others", "Bulgaria", "+359877700247", "Test Message");
        SubmitContactForm();
        Assert.That(IsValidationErrorDisplayed(), Is.EqualTo("Invalid email format"), "Invalid email error message is not displayed correctly!");
    }

    [Test]
    public void CheckCounterOfMessageField()
    {
        string charNumber = new string('a', 1999);
        FillContactForm("Chef – DevOps", "test@progress.com", "Test First Name", "Test Last Name", "TestCompany",
        "Others", "Bulgaria", "+359877700247", charNumber);

        var counter = driver.FindElement(By.CssSelector("span.TxtCounter-Number"));
        int correctCounter = 2000 - charNumber.Length;

        Assert.That(correctCounter, Is.EqualTo(Int32.Parse(counter.Text)), "Counter is not correct!");
    }

    [Test]
    public void SubmitFormWithSpecialSymbolInName()
    {
        FillContactForm("Chef – DevOps", "test@progress.com", "First Name$", "Test Last Name", "TestCompany",
        "Others", "Bulgaria", "+359877700247", "Test Message");
        SubmitContactForm();
        Assert.That(IsValidationErrorDisplayed(), Is.EqualTo("Invalid format"), "Invalid format error message is not displayed correctly!");
    }

    [Test]
    public void SubmitFormWithEmptyState()
    {
       FillContactForm("Chef – DevOps", "test@progress.com", "Test First Name", "Test Last Name", "TestCompany",
        "Others", "USA", "+359877700247", "Test Message", "");
        SubmitContactForm();
        Assert.That(IsValidationErrorDisplayed(), Is.EqualTo("State is required"), "Is Required error message is not displayed correctly!");
    }

    [Test]
    public void OpenPrivacyPolicyInNewWindow()
    {
        FillContactForm("Chef – DevOps", "test@progress.com", "Test First Name", "Test Last Name", "TestCompany",
        "Others", "Bulgaria", "+359877700247", "Test Message");

        var cookieButton = driver.FindElement(By.Id("onetrust-reject-all-handler"));
        wait.Until(ExpectedConditions.ElementToBeClickable(cookieButton));
        cookieButton.Click();

        var privacyPolicyLink = wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText("Privacy Policy")));
        privacyPolicyLink.Click();  

        driver.SwitchTo().Window(((IList<string>)[.. driver.WindowHandles])[1]);

        string url = driver.Url;

        Assert.That(url, Is.EqualTo("https://www.progress.com/legal/privacy-policy"), "New windows is not opened or url is not correct!");
    }

    [TearDown]
    public void TearDown()
    {
        driver.Quit();
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
        //throw new NoSuchElementException();
    }
    
    // Fill the contact form
    private void FillContactForm(string product, string email, string firstName, string lastName,
    string company, string jobTitle, string country, string phone, string message, string state = "")
    {
            var productField = driver.FindElement(By.Id("Dropdown-1"));
            var emailField = driver.FindElement(By.Id("Email-1"));
            var firstNameField = driver.FindElement(By.Name("FirstName"));
            var lastNameField = driver.FindElement(By.Name("LastName"));
            var companyField = driver.FindElement(By.Name("CompanyName"));
            var jobTitleField = driver.FindElement(By.Id("Dropdown-2"));
            var countryField = driver.FindElement(By.Id("Country-1"));
            var phoneField = driver.FindElement(By.Id("Textbox-5"));
            var messageField = driver.FindElement(By.Id("Textarea-1"));
            
            // Fill each of the fields according to its type
            var selectedProduct = new SelectElement(productField);
            selectedProduct.SelectByText(product);

            var selectedJob = new SelectElement(jobTitleField);
            selectedJob.SelectByText(jobTitle);

            var selectedCountry = new SelectElement(countryField);
            selectedCountry.SelectByText(country);

            if (!string.IsNullOrEmpty(state))
            {
                wait.Until(driver =>
                {
                    var stateField = driver.FindElement(By.Id("State-1"));
                    var selectedState = new SelectElement(stateField);
                    selectedState.SelectByText(state);
                    return selectedState.Options.Count > 0;
                });
            }
            else if(country == "USA" || country == "Canada" )
            {
                Console.WriteLine("You must specify a state!");
            }

            // Clear fields from previous data
            emailField.Clear();
            firstNameField.Clear();
            lastNameField.Clear();
            companyField.Clear();
            phoneField.Clear();
            messageField.Clear();

            // Put the data in the fields
            emailField.SendKeys(email);
            firstNameField.SendKeys(firstName);
            lastNameField.SendKeys(lastName);
            companyField.SendKeys(company);
            phoneField.SendKeys(phone);
            messageField.SendKeys(message);
    }

    // Submit the contact form
    private void SubmitContactForm()
    {
        var cookieButton = driver.FindElement(By.Id("onetrust-reject-all-handler"));
        var contactSalesButton = driver.FindElement(By.CssSelector("button.Btn.Btn--prim.-db.js-submit"));
        wait.Until(ExpectedConditions.ElementToBeClickable(cookieButton));
        cookieButton.Click();
        wait.Until(ExpectedConditions.ElementToBeClickable(contactSalesButton));
        contactSalesButton.Click();
    }

    // Checking the new page after the successful submision of form
    private bool SubmitSuccessfully()
    {
        try
        {
            var contactThankYou = wait.Until(driver => driver.FindElement(By.Id("Content_C055_Col00")));

            return contactThankYou.Displayed;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    // Check if a validation error is displayed
     private string IsValidationErrorDisplayed()
    {
        var actualErrorMessage = wait.Until(driver => driver.FindElement(By.CssSelector("p.sfError")));

        return actualErrorMessage.Text;
    }

}
