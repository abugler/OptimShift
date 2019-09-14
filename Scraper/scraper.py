from requests import get, post
from requests.exceptions import RequestException
from bs4 import BeautifulSoup
from bs4 import NavigableString
from selenium import webdriver
from contextlib import closing
from datetime import datetime

class Scraper:

    login_url = "https://whentowork.com/logins.htm"
    input_prompt = "Please log into WhenToWork using this link: \n" + \
                          login_url + \
                          " \nAfter logging in, Click \"Employees\", " + \
                          "then click on the pencil-shaped edit icon for any employee." + \
                          "\nA popup will then appear. Click Work Time Prefs, and another popup will appear." + \
                            "\nCopy and paste the URL of that last popup here:\n"

    """
    Debugging constructor
    """
    def __init__(self, url):
        self.given_url = url
        self.new_availability_file = "AVAILABILITY_" + str(datetime.now()) + ".txt"
        with open(self.new_availability_file, "w") as file:
            file.write("EMPLOYEES")

    @classmethod
    def obtain_initial_pref_url(cls):
        # Ask the user to input the pref url
        # I couldn't do this automatically, W2W didn't like my post requests :(

        return input("Please log into WhenToWork using this link: \n" +
                          login_url +
                          " \nAfter logging in, Click \"Employees\", " +
                          "then click on the pencil-shaped edit icon for any employee." +
                          "\nA popup will then appear. Click Work Time Prefs, and another popup will appear." +
                            "\nCopy and paste the URL of that last popup here:\n")

    """
    Production Constructor
    """
    @classmethod
    def create_scraper(cls):
        return Scraper(cls.obtain_initial_pref_url())

    pref_colors = {
        u"#90f68e": 3,
        u"#ffffff": 2,
        u"#ffaeae": 1,
        u"#ff0000": 0
    }

    w2w_root_url = "https://www4.whentowork.com"
    w2w_left_title = "Click to view Previous Employee"
    w2w_right_title = "Click to view Next Employee"

    no_table_found_error = "Could not find the HTML table object.  Possible issues: \n" + \
                            "1. The Session key has expired.  This results in OptimShift not being able to access the page. Please try to log into When To Work again\n" + \
                            "2. You did not submit a correct prefs link.  Please ensure that the link you submitted is a correct prefs link."

    """
    Scrapes Availabilities of a single employee
    """
    def scrape_availabilities(self, scrape_left, scrape_right):
        if len(self.given_url) == 0:
            raise ValueError("Preferences URL is not initialized")

        """
        Attempts to get the content at `url` by making an HTTP GET request.
        If the content-type of response is some kind of HTML/XML, return the
        text content, otherwise return None.
        """

        """
        session = HTMLSession()
        resp = session.get(self.given_url)
        resp.html.render()
        raw_html_content = resp.html
        """
        driver = webdriver.PhantomJS()
        driver.get(self.given_url)
        raw_html_content = driver.page_source
        html_soup = BeautifulSoup(raw_html_content, 'html.parser')
        print(html_soup.contents)
        # Collect Name
        name = str(html_soup.find("td", class_="poptitle").string)

        # Collect Availabilities
        table = html_soup.find("table", id="PrefTable")
        if not table:
            raise Exception(Scraper.no_table_found_error)
        rows = list(table.children)[1].children
        availabilities = []
        for row in rows:
            if type(row) == NavigableString or str(row.get("ud")) == "FirstRow":
                continue
            for tile in row.children:
                if type(tile) == NavigableString or (("dn" in tile["class"] or "dnsp" in tile["class"]) and "spacing" in tile["class"]):
                    continue
                availabilities.append(Scraper.pref_colors.get(tile["bgcolor"]))

        # Write Availabilities to file
        with open(self.new_availability_file, "a") as file:
            file.write("\n")
            file.write("NAME: " + name + "\n")
            file.write("AVAILABILITY" + "\n")

            currentAvailability = availabilities[0]
            time = 0
            for a in availabilities:
                if a != currentAvailability:
                    file.write(str(time) + "\t" + str(currentAvailability) + "\n")
                    currentAvailability = a
                    time = 0
                time += 15
            file.write(str(time) + "\t" + str(currentAvailability) + "\n")

        # Find the next prefs to scrape.
        if scrape_left:
            next_left = html_soup.find("a", title=Scraper.w2w_left_title)
            if next_left is not None:
                next_left_url = next_left["href"]
                self.given_url = Scraper.w2w_root_url + next_left_url
                self.scrape_availabilities(True, False)
        if scrape_right:
            next_right = html_soup.find("a", title=Scraper.w2w_right_title)
            if next_right is not None:
                next_right_url = next_right["href"]
                self.given_url = Scraper.w2w_root_url + next_right_url
                self.scrape_availabilities(False, True)

    def scrape_all(self):
        self.scrape_availabilities(True, True)

    """
    Gets the HTML object from the website, and returns it.
    """
    def raw_get_content(self):
        def is_good_response(resp):
            """
            Returns True if the response seems to be HTML, False otherwise.
            """
            content_type = resp.headers['Content-Type'].lower()
            return (resp.status_code == 200
                    and content_type is not None
                    and content_type.find('html') > -1)

        try:
            with closing(get(self.given_url, stream=True)) as resp:
                if is_good_response(resp):
                    return resp.content
                else:
                    print("Nothing was returned")
                    return None

        except RequestException as e:
            print('Error during requests to {0} : {1}'.format(self.given_url, str(e)))
            return None



