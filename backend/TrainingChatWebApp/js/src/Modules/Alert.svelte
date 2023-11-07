<script>
    import Materialize from "materialize-css"
    
    export let alert_text;
    export let is_visible = false;
</script>

<div class="card-panel" class:hide={!is_visible}>
    <pre>{alert_text}</pre>
</div>

<style>
    .card-panel {
        background-color: var(--color, #EF9A9A);
    }
</style>
