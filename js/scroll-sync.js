window.scrollSync = {
    navLinks: null,
    sections: null,
    clickedSection: null,
    clickTimeout: null,

    init: function () {
        this.sections = Array.from(document.querySelectorAll('section[id]'));
        this.navLinks = document.querySelectorAll('.toc-nav a');

        if (this.sections.length === 0 || this.navLinks.length === 0) return;

        this.navLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                const targetId = link.getAttribute('href').substring(1);
                this.clickedSection = targetId;
                this.setActive(targetId);

                if (this.clickTimeout) clearTimeout(this.clickTimeout);
                this.clickTimeout = setTimeout(() => {
                    this.clickedSection = null;
                }, 1000);
            });
        });

        let ticking = false;
        window.addEventListener('scroll', () => {
            if (!ticking) {
                window.requestAnimationFrame(() => {
                    this.updateActiveFromScroll();
                    ticking = false;
                });
                ticking = true;
            }
        });

        this.updateActiveFromScroll();
    },

    updateActiveFromScroll: function () {
        if (!this.sections || !this.navLinks) return;

        if (this.clickedSection) {
            this.setActive(this.clickedSection);
            return;
        }

        const scrollY = window.scrollY;
        const windowHeight = window.innerHeight;
        const docHeight = document.documentElement.scrollHeight;

        if (scrollY + windowHeight >= docHeight - 20) {
            this.setActive(this.sections[this.sections.length - 1].id);
            return;
        }

        const offset = 100;
        let activeSection = this.sections[0].id;

        for (let i = 0; i < this.sections.length; i++) {
            const section = this.sections[i];
            const rect = section.getBoundingClientRect();

            if (rect.top <= offset) {
                activeSection = section.id;
            }
        }

        this.setActive(activeSection);
    },

    setActive: function (sectionId) {
        this.navLinks.forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('href') === '#' + sectionId) {
                link.classList.add('active');
            }
        });
    },

    dispose: function () {
        if (this.clickTimeout) clearTimeout(this.clickTimeout);
    }
};
