import unittest
from scraper import Scraper


class MyTestCase(unittest.TestCase):
    """
    def test_obtain_url(self):
        s = Scraper.create_scraper()

    def test_content_test(self):
        s = Scraper("https://www4.whentowork.com/cgi-bin/w2wD.dll/mgrempinfopop?SID=10563269224186&from=&EmployeeId=187756949&Ref=Y")
        s.raw_get_content()
    """

    def test_obtain_avail(self):
        # s = Scraper.create_scraper()
        # Can't really do inputs correctly due to PyCharm Deficiencies.  Hardcoding the test
        s = Scraper("https://www4.whentowork.com/cgi-bin/w2wDD.dll/mgrempinfopop?SID=4614665794186&from=&EmployeeId=220944566&Ref=Y")
        s.scrape_all()

if __name__ == '__main__':
    unittest.main()
