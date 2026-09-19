// StarLight Global Interactions & Preference Management
document.addEventListener('DOMContentLoaded', () => {
    // Restore preferred branch from localStorage
    const savedBranch = localStorage.getItem('starlight_preferred_branch');
    if (savedBranch) {
        document.querySelectorAll('.branch-select-btn').forEach(button => {
            const cinemaName = button.getAttribute('data-branch-name');
            if (cinemaName === savedBranch) {
                applyBranchSelectedState(button, true);
            }
        });
    }
});

function handleSelectBranch(button, branchName) {
    const isSelected = button.classList.contains('bg-primary-container');
    
    // Clear all other branch buttons
    document.querySelectorAll('.branch-select-btn').forEach(btn => {
        applyBranchSelectedState(btn, false);
    });

    if (!isSelected) {
        applyBranchSelectedState(button, true);
        localStorage.setItem('starlight_preferred_branch', branchName);
        showStarlightToast(`Selected "${branchName}" as your preferred cinema!`);
    } else {
        localStorage.removeItem('starlight_preferred_branch');
        showStarlightToast(`Cleared preferred cinema selection.`);
    }
}

function applyBranchSelectedState(button, selected) {
    const label = button.querySelector('.select-label');
    const icon = button.querySelector('.material-symbols-outlined');
    if (!label || !icon) return;

    if (selected) {
        button.classList.remove('bg-surface-container', 'text-on-surface');
        button.classList.add('bg-primary-container', 'text-on-primary-container', 'shadow-[0_0_12px_rgba(229,9,20,0.4)]');
        label.textContent = 'Preferred';
        icon.textContent = 'check';
    } else {
        button.classList.add('bg-surface-container', 'text-on-surface');
        button.classList.remove('bg-primary-container', 'text-on-primary-container', 'shadow-[0_0_12px_rgba(229,9,20,0.4)]');
        label.textContent = 'Select Branch';
        icon.textContent = 'bookmark_add';
    }
}

function showStarlightToast(message) {
    let toast = document.getElementById('starlight-toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'starlight-toast';
        toast.className = 'fixed bottom-6 right-6 z-50 bg-surface-container-high border border-primary/30 text-on-surface px-5 py-3 rounded-xl shadow-2xl flex items-center gap-3 transition-all duration-300 transform translate-y-8 opacity-0';
        document.body.appendChild(toast);
    }
    toast.innerHTML = `
        <span class="material-symbols-outlined text-primary-container text-xl">verified</span>
        <span class="font-label-md text-sm">${message}</span>
    `;
    toast.classList.remove('translate-y-8', 'opacity-0');
    toast.classList.add('translate-y-0', 'opacity-100');

    setTimeout(() => {
        toast.classList.add('translate-y-8', 'opacity-0');
        toast.classList.remove('translate-y-0', 'opacity-100');
    }, 3200);
}
