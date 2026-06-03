// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(() => {
    const componentSelector = '[data-pill-select]';
    const openClass = 'show';

    function closeAll(except = null) {
        document.querySelectorAll(componentSelector).forEach(component => {
            if (component !== except) {
                component.classList.remove(openClass);
                const button = component.querySelector('[data-pill-select-button]');
                if (button) button.setAttribute('aria-expanded', 'false');
            }
        });
    }

    function syncLabel(component) {
        const button = component.querySelector('[data-pill-select-button]');
        const input = component.querySelector('[data-pill-select-input]');
        const option = component.querySelector(`[data-pill-select-option][data-value="${CSS.escape(input?.value ?? '')}"]`);

        if (!button || !input) return;

        const placeholder = component.dataset.placeholder || button.dataset.placeholder || 'Seleccione';
        button.textContent = option?.dataset.label || option?.textContent?.trim() || placeholder;
        button.dataset.selectedValue = input.value;
    }

    function openComponent(component) {
        closeAll(component);
        component.classList.add(openClass);
        const button = component.querySelector('[data-pill-select-button]');
        if (button) button.setAttribute('aria-expanded', 'true');
    }

    function closeComponent(component) {
        component.classList.remove(openClass);
        const button = component.querySelector('[data-pill-select-button]');
        if (button) button.setAttribute('aria-expanded', 'false');
    }

    function toggleComponent(component) {
        if (component.classList.contains(openClass)) {
            closeComponent(component);
        } else {
            openComponent(component);
        }
    }

    function setValue(component, value, label) {
        const input = component.querySelector('[data-pill-select-input]');
        if (!input) return;

        input.value = value;
        input.dispatchEvent(new Event('change', { bubbles: true }));
        input.dispatchEvent(new Event('input', { bubbles: true }));

        const button = component.querySelector('[data-pill-select-button]');
        if (button) {
            button.textContent = label;
            button.dataset.selectedValue = value;
        }

        if (value) {
            input.setCustomValidity('');
        } else if (component.dataset.required === 'true') {
            input.setCustomValidity(component.dataset.requiredMessage || 'Seleccione una opción.');
        }
    }

    function initComponent(component) {
        const input = component.querySelector('[data-pill-select-input]');
        const button = component.querySelector('[data-pill-select-button]');
        if (!input || !button) return;

        syncLabel(component);

        button.addEventListener('click', event => {
            event.preventDefault();
            event.stopPropagation();
            toggleComponent(component);
        });

        component.querySelectorAll('[data-pill-select-option]').forEach(option => {
            option.addEventListener('click', event => {
                event.preventDefault();
                event.stopPropagation();
                setValue(component, option.dataset.value || '', option.dataset.label || option.textContent.trim());
                closeComponent(component);
            });
        });

        input.addEventListener('invalid', () => {
            if (component.dataset.required === 'true' && !input.value) {
                input.setCustomValidity(component.dataset.requiredMessage || 'Seleccione una opción.');
            }
        });

        input.addEventListener('change', () => {
            syncLabel(component);
        });

        component.addEventListener('keydown', event => {
            if (event.key === 'Escape') {
                closeComponent(component);
                button.focus();
            }
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll(componentSelector).forEach(initComponent);

        document.addEventListener('click', event => {
            const target = event.target;
            const inside = target instanceof Element ? target.closest(componentSelector) : null;
            if (!inside) {
                closeAll();
            }
        });
    });
})();
