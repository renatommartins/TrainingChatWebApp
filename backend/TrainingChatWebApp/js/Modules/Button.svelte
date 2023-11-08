<script>
    import Materialize from "materialize-css"
    
    export let button_label;
    export let enabled = true;
</script>

<button class="waves-effect btn-small blue-grey" on:click disabled={!enabled}>{button_label}</button>

<style>
    
</style>
