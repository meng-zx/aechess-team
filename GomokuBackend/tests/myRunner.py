from django.test.runner import DiscoverRunner


class MyRunner(DiscoverRunner):
    def __init__(self, *args, **kwargs):
        super().__init__(verbosity=2, keepdb=True)
